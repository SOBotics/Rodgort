using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NLog;
using Rodgort.Data;
using Rodgort.Services;
using Rodgort.Services.HostedServices;
using StackExchangeApi;
using StackExchangeChat;
using StackExchangeChat.Utilities;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Rodgort
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("RodgortDB");
            GlobalDiagnosticsContext.Set("connectionString", connectionString);

            services.AddHangfire(config => config.UsePostgreSqlStorage(connectionString));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            var symmetricKey = Convert.FromBase64String(Configuration["JwtSigningKey"]);
            services.AddAuthentication(o =>
                {
                    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(o =>
                {
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        ValidateIssuerSigningKey = true,

                        IssuerSigningKey = new SymmetricSecurityKey(symmetricKey)
                    };
                });

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            services
                .AddEntityFrameworkNpgsql()
                .AddDbContext<RodgortContext>(options =>
                {
                    options.UseNpgsql(connectionString).ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));
                });

            services.AddTransient<DateService>();
            services.AddTransient<ApiClient>();
            services.AddTransient<BurnakiFollowService>();
            services.AddTransient<BurnProcessingService>();
            services.AddTransient<BurnCatchupService>();
            services.AddTransient<MetaCrawlerService>();
            services.AddTransient<TagCountService>();

            services.AddTransient(_ => new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }));
            services.AddTransient(_ => new HttpClientWithHandler(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }));

            var chatCredentials = new ChatCredentials();
            Configuration.Bind("ChatCredentials", chatCredentials);

            var stackExchangeApiCredentials = new StackExchangeApiCredentials();
            Configuration.Bind("StackExchangeApiCredentials", stackExchangeApiCredentials);

            services.AddSingleton<IChatCredentials>(_ => chatCredentials);
            services.AddSingleton<IStackExchangeApiCredentials>(_ => stackExchangeApiCredentials);

            services.AddScoped<SiteAuthenticator>();
            services.AddScoped<ChatClient>();
            services.AddScoped<NewBurninationService>();

            services.AddSingleton<IHostedService, BurnakiFollowService>();
            services.AddSingleton<IHostedService, LiveMetaQuestionWatcherService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAuthentication();
            app.UseHangfireServer();
            app.UseHangfireDashboard(options: new DashboardOptions
            {
                Authorization = new List<IDashboardAuthorizationFilter> { new NoAuthorizationFilter() }
            });

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();


            app.UseWebSockets();
            app.ConfigureQuotaRemainingWebsocket();
            
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseMiddleware(typeof(ExceptionHandlerMiddleware));

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "default",
                    "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                if (env.IsDevelopment())
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
            });

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            using (var context = serviceScope.ServiceProvider.GetService<RodgortContext>())
                if (!context.Database.IsInMemory())
                    context.Database.Migrate();
            
            // Every hour
            RecurringJob.AddOrUpdate<MetaCrawlerService>(MetaCrawlerService.SERVICE_NAME, service => service.CrawlMetaSync(), "0 * * * *");

            // Every hour
            RecurringJob.AddOrUpdate<TagCountService>(TagCountService.ALL_TAGS, service => service.GetQuestionCountForApprovedTagsSync(), "5 * * * *");

            // Every day
            RecurringJob.AddOrUpdate<UserDisplayNameService>(UserDisplayNameService.SYNC_ALL_USERS, service => service.SyncAllUsersSync(), "10 0 * * *");

            // Every day
            RecurringJob.AddOrUpdate<TrogdorRoomOwnerService>(TrogdorRoomOwnerService.SERVICE_NAME, service => service.SyncTrogdorRoomOwnersSync(), "15 0 * * *");

            RecurringJob.AddOrUpdate<BurnCatchupService>(BurnCatchupService.SERVICE_NAME, service => service.CatchupSync(), "20 0 * * *");

            RecurringJob.AddOrUpdate<InitialTagQueryService>(InitialTagQueryService.SERVICE_NAME, service => service.QuerySync(), "25 0 * * *");

            RecurringJob.AddOrUpdate<InitialTagQueryService>(InitialTagQueryService.SERVICE_NAME, service => service.QuerySync(), "30 0 * * *");

            RecurringJob.AddOrUpdate<SynonymSynchroniserService>(SynonymSynchroniserService.SERVICE_NAME, service => service.SynchroniseSync(), "35 0 * * *");

            // I don't really want this to automatically execute, but the 'never' crontab expression doesn't work for hangfire.
            // So, we'll just execute once a year - the first of January at 0:10
            RecurringJob.AddOrUpdate<BurninationTagGuessingService>(BurninationTagGuessingService.SERVICE_NAME, service => service.GuessTags(), "20 0 1 1 *");
            RecurringJob.AddOrUpdate<UserDisplayNameService>(UserDisplayNameService.SYNC_USERS_NO_NAME, service => service.SyncUsersWithNoNameSync(), "25 0 1 1 *");
        }

        public class NoAuthorizationFilter : IDashboardAuthorizationFilter
        {
            public bool Authorize(DashboardContext context)
            {
                return true;
            }
        }
    }
}
