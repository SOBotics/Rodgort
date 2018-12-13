using System.Net;
using System.Net.Http;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Rodgort.Data;
using Rodgort.Services;
using StackExchangeApi;

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
            services.AddTransient(_ => new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseHangfireServer();
            app.UseHangfireDashboard();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler();
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

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
            
            RecurringJob.AddOrUpdate<MetaCrawlerService>(MetaCrawlerService.SERVICE_NAME, service => service.CrawlMeta(), "0 0 * * 0");
            RecurringJob.AddOrUpdate<TagCountService>(TagCountService.SERVICE_NAME, service => service.GetQuestionCount(), "0 0 * * 0");

            // I don't really want this to automatically execute, but the 'never' crontab expression doesn't work for hangfire.
            // So, we'll just execute once a year
            RecurringJob.AddOrUpdate<BurninationTagGuessingService>(BurninationTagGuessingService.SERVICE_NAME, service => service.GuessTags(), "0 0 1 1 *");
        }
    }
}
