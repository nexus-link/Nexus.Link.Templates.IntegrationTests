using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using DataAccess.Memory;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Service.Mapping;
using SharedKernel;

namespace Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            var nexusSetting = Configuration.GetSection("NexusSettings").Get<NexusSettings>();
            var tenant = new Tenant(nexusSetting.Organization, nexusSetting.Environment);
            var runTimeLevel = (RunTimeLevelEnum)Enum.Parse(typeof(RunTimeLevelEnum), nexusSetting.RunTimeLevel);
            FulcrumApplicationHelper.RuntimeSetup(nexusSetting.ApplicationName, tenant, runTimeLevel);

            // TODO: Logging

            services.AddControllers();
            services.AddMvc().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.IgnoreNullValues = true;
                options.JsonSerializerOptions.WriteIndented = true;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = Configuration["ApiName"], Version = "v1" });
                c.EnableAnnotations();
                c.TagActionsBy(api => new List<string> { api.GroupName });
            });

            // TODO: Patch if using database

            // TODO: Check configuration if we're using database
            services.AddSingleton<IStorage, MemoryStorage>();
            services.AddSingleton<ITestLogic, TestLogic>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", Configuration["ApiName"]);
            });
        }
    }
}
