using System.Collections.Generic;
using System.Text.Json.Serialization;
using DataAccess.Memory;
using DataAccess.TableStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound;
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
            services
                .AddMvc()
                .AddMvcOptions(options => options.EnableEndpointRouting = false)
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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = Configuration["ApiName"], Version = "v1" });
                c.EnableAnnotations();
                c.TagActionsBy(api => new List<string> { api.GroupName });
            });

            var connectionString = Configuration["ConnectionString"];
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                var tableStorage = new TableStorage(connectionString);
                services.AddSingleton<IStorage>(tableStorage);
            }
            else
            {
                services.AddSingleton<IStorage, MemoryStorage>();
            }
            services.AddSingleton<ITestLogic, TestLogic>();
        }

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
