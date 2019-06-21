using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
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
using Rodgort.Data.Tables;
using Rodgort.Services;
using Rodgort.Services.HostedServices;
using Rodgort.Utilities;
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
            services.AddResponseCompression();

            var connectionString = Configuration.GetConnectionString("RodgortDB");
            GlobalDiagnosticsContext.Set("connectionString", connectionString);

            services.AddHangfire(config =>
            {
                config.UsePostgreSqlStorage(connectionString);
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddAuthentication(o =>
                {
                    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(o => { o.TokenValidationParameters = GetTokenValidationParameters(); });
            
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            services
                .AddEntityFrameworkNpgsql()
                .AddDbContext<RodgortContext>(options =>
                {
                    options.UseNpgsql(connectionString, opts => opts.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds)).ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));
                });

            services.AddTransient<DateService>();
            services.AddTransient<ApiClient>();
            services.AddTransient<BurnakiFollowService>();
            services.AddTransient<BurnProcessingService>();
            services.AddTransient<BurnCatchupService>();
            services.AddTransient<MetaCrawlerService>();
            services.AddTransient<TagCountService>();
            services.AddTransient<BurninationTagGuessingService>();

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

            services.AddSingleton<ObservableClientWebSocket>();
            services.AddSingleton<IHostedService, BurnakiFollowService>();
            services.AddSingleton<IHostedService, LiveMetaQuestionWatcherService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseResponseCompression();
            app.UseAuthentication();
            app.UseHangfireServer();
            app.UseHangfireDashboard(options: new DashboardOptions
            {
                Authorization = new List<IDashboardAuthorizationFilter> { new CookieAuthorizationFilter(GetTokenValidationParameters()) }
            });

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();


            app.UseWebSockets();
            app.ConfigureWebsockets();
            
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
            RecurringJob.AddOrUpdate<TagCountService>(TagCountService.ALL_TAGS, service => service.GetQuestionCountForApprovedTagsSync(), "5 * * * *");

            // Every day
            RecurringJob.AddOrUpdate<MetaCrawlerService>(MetaCrawlerService.SERVICE_NAME, service => service.CrawlMetaSync(), "0 0 * * *");

            RecurringJob.AddOrUpdate<UserDisplayNameService>(UserDisplayNameService.SYNC_ALL_USERS, service => service.SyncAllUsersSync(), "10 0 * * *");

            RecurringJob.AddOrUpdate<RoleSyncService>(RoleSyncService.SERVICE_NAME, service => service.SyncRolesSync(), "15 0 * * *");

            RecurringJob.AddOrUpdate<BurnCatchupService>(BurnCatchupService.SERVICE_NAME, service => service.CatchupSync(), "20 0 * * *");

            RecurringJob.AddOrUpdate<InitialTagQueryService>(InitialTagQueryService.SERVICE_NAME, service => service.QuerySync(), "25 0 * * *");

            RecurringJob.AddOrUpdate<SynonymSynchroniserService>(SynonymSynchroniserService.SERVICE_NAME, service => service.SynchroniseSync(), "35 0 * * *");

            RecurringJob.AddOrUpdate<LogCleanupService>(LogCleanupService.SERVICE_NAME, service => service.Execute(), "40 0 * * *");

            // I don't really want this to automatically execute, but the 'never' crontab expression doesn't work for hangfire.
            // So, we'll just execute once a year - the first of January at 0:10
            RecurringJob.AddOrUpdate<UserDisplayNameService>(UserDisplayNameService.SYNC_USERS_NO_NAME, service => service.SyncUsersWithNoNameSync(), "55 0 1 1 *");
        }

        private TokenValidationParameters GetTokenValidationParameters()
        {
            var symmetricKey = Convert.FromBase64String(Configuration["JwtSigningKey"]);
            return new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,

                IssuerSigningKey = new SymmetricSecurityKey(symmetricKey)
            };
        }

        public class CookieAuthorizationFilter : IDashboardAuthorizationFilter
        {
            private readonly TokenValidationParameters _tokenValidationParameters;

            public CookieAuthorizationFilter(TokenValidationParameters tokenValidationParameters)
            {
                _tokenValidationParameters = tokenValidationParameters;
            }

            public bool Authorize(DashboardContext context)
            {
                var httpContext = context.GetHttpContext();
                if (httpContext.Request.Cookies.TryGetValue("access_token", out var authCookie))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var principal = handler.ValidateToken(authCookie, _tokenValidationParameters, out var validToken);

                    return validToken is JwtSecurityToken && principal.HasRole(DbRole.ADMIN);
                }

                return false;
            }
        }
    }
}
