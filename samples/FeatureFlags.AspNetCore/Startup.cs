﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RimDev.AspNetCore.FeatureFlags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.AspNetCore
{
    public class Startup
    {
        private readonly FeatureFlagOptions options;

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            options = new FeatureFlagOptions()
                .UseInMemoryFeatureProvider();
                // .UseCachedSqlFeatureProvider(Configuration.GetConnectionString("localDb"));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddFeatureFlags(options);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseFeatureFlags(options);
            app.UseFeatureFlagsUI(options);

            app.Map("/test-feature", appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    var testFeature = context.RequestServices.GetService<TestFeature>();

                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync($@"
                        {testFeature.GetType().Name}: {testFeature.Value}
                        <br />
                        <a href=""{options.UiPath}"">View UI</a>");
                });
            });

            app.Map("", appBuilder =>
            {
                appBuilder.Run(context =>
                {
                    context.Response.Redirect("/test-feature");

                    return Task.CompletedTask;
                });
            });
        }
    }
}
