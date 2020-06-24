using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebApplication13
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

            var metrics = new MetricsBuilder()
                         .Configuration.Configure(
                             options =>
                             {
                                 options.AddServerTag();
                                 options.AddEnvTag();
                                 options.AddAppTag("BP4");
                             })
                         .OutputMetrics.AsPrometheusPlainText()
                         .Build();

            services.AddMetrics(metrics);
            services.AddMetricsReportingHostedService();
            services.AddMetricsEndpoints();
            services.AddMetricsTrackingMiddleware();
            services.AddMvcCore().AddMetricsCore();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseMetricsAllEndpoints();
            

            app.UseMetricsApdexTrackingMiddleware();
            app.UseMetricsRequestTrackingMiddleware();
            app.UseMetricsErrorTrackingMiddleware();
            app.UseMetricsActiveRequestMiddleware();
            app.UseMetricsPostAndPutSizeTrackingMiddleware();
            app.UseMetricsOAuth2TrackingMiddleware();
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }


}
