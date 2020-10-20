using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using DataAccess.Memory;
using DataAccess.Sql;
using DataAccess.TableStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Nexus.Link.Authentication.AspNet.Sdk.Handlers;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound;
using Service.Configuration;
using Service.Logic;
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

            services.AddAuthentication("Basic").AddScheme<TokenAuthenticationOptions, TokenAuthenticationHandler>("Basic", o => { });

            services
                .AddMvc(config =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    config.Filters.Add(new AuthorizeFilter(policy));
                })
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

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = ApiName, Version = "v1" });
                c.EnableAnnotations();
                c.TagActionsBy(api => new List<string> { api.GroupName });

                var xmlFile = Path.ChangeExtension(typeof(Startup).Assembly.Location, ".xml");
                c.IncludeXmlComments(xmlFile);

                c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "basic",
                    In = ParameterLocation.Header,
                    Description = "Basic Authorization header using the Bearer scheme."
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "basic"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            var sqlConnectionString = Configuration["Service:SqlConnectionString"];
            var storageConnectionString = Configuration["Service:StorageConnectionString"];
            if (!string.IsNullOrWhiteSpace(sqlConnectionString))
            {
                var sqlStorage = new SqlStorage(sqlConnectionString);
                services.AddSingleton<IStorage>(sqlStorage);
                new DatabasePatcherHandler(HostEnvironment.ContentRootPath).PatchIfNecessary(Configuration["Nexus:Environment"], sqlConnectionString, Configuration["Service:MasterConnectionString"]);
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

            // Handles token validation and sets FulcrumApplication.Context.CallingClientName
            var nexusSettings = Configuration.GetSection("Nexus").Get<NexusSettings>();
            app.UseNexusTokenValidationHandler(nexusSettings.PublicKey);

            app.UseHttpsRedirection();
            app.UseAuthentication();
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
