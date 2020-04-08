using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using DataAccess.Memory;
using DataAccess.Sql;
using DataAccess.TableStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound;
using Service.Mapping;
using SharedKernel;

namespace Service
{
    /// <summary></summary>
    public class Startup
    {
        /// <summary></summary>
        public Startup(IConfiguration configuration, IWebHostEnvironment hostEnvironment)
        {
            Configuration = configuration;
            HostEnvironment = hostEnvironment;
        }

        /// <summary></summary>
        public IConfiguration Configuration { get; }
        /// <summary></summary>
        public IWebHostEnvironment HostEnvironment { get; }

        /// <summary></summary>
        public static string ApiName;


        /// <summary></summary>
        public void ConfigureServices(IServiceCollection services)
        {
            ApiName = Configuration["ApiName"];

            services
                .AddMvc()
                .AddMvcOptions(options =>
                {
                    options.EnableEndpointRouting = false;
                    options.AllowEmptyInputInBodyModelBinding = true;
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                    options.JsonSerializerOptions.WriteIndented = true;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = ApiName, Version = "v1" });
                c.EnableAnnotations();
                c.TagActionsBy(api => new List<string> { api.GroupName });
                
                var xmlFile = Path.ChangeExtension(typeof(Startup).Assembly.Location, ".xml");
                c.IncludeXmlComments(xmlFile);
            });

            var sqlConnectionString = Configuration["SqlConnectionString"];
            var storageConnectionString = Configuration["StorageConnectionString"];
            if (!string.IsNullOrWhiteSpace(sqlConnectionString))
            {
                var sqlStorage = new SqlStorage(sqlConnectionString);
                services.AddSingleton<IStorage>(sqlStorage);
                new DatabasePatcherHandler(HostEnvironment.ContentRootPath).PatchIfNecessary(Configuration["Environment"], sqlConnectionString, Configuration["MasterConnectionString"]);
            }
            else if (!string.IsNullOrWhiteSpace(storageConnectionString))
            {
                var tableStorage = new TableStorage(storageConnectionString);
                services.AddSingleton<IStorage>(tableStorage);
            }
            else
            {
                services.AddSingleton<IStorage, MemoryStorage>();
            }
            services.AddSingleton<ITestLogic, TestLogic>();
        }

        /// <summary></summary>
        public void Configure(IApplicationBuilder app)
        {
            // Get the correlation ID from the request header and store it in FulcrumApplication.Context
            app.UseNexusSaveCorrelationId();
            // Start and stop a batch of logs, see also Nexus.Link.Libraries.Core.Logging.BatchLogger.
            app.UseNexusBatchLogs();
            // Log all requests and responses
            app.UseNexusLogRequestAndResponse();
            // Convert exceptions into error responses (HTTP status codes 400 and 500)
            app.UseNexusExceptionToFulcrumResponse();

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", Configuration["ApiName"]);
            });

            app.UseMvc();
        }
    }
}
