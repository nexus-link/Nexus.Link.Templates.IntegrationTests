using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Mocks.Helpers;
using Newtonsoft.Json.Converters;
using Nexus.Link.Authentication.AspNet.Sdk.Handlers;
using Nexus.Link.Authentication.Sdk;
using Nexus.Link.KeyTranslator.Sdk;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Clients;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound;
using Nexus.Link.Libraries.Web.Platform.Authentication;
using Nexus.Link.Libraries.Web.RestClientHelper;
using FulcrumApplicationHelper = Nexus.Link.Libraries.Web.AspNet.Application.FulcrumApplicationHelper;

namespace Mocks
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
            var nexusSettings = Configuration.GetSection("Nexus");
            FulcrumApplicationHelper.WebBasicSetup(nexusSettings);

            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.UseCamelCasing(true);
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mocks", Version = "v1" });
            });

            ConfigureNexus(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseNexusSaveCorrelationId();
            app.UseNexusBatchLogs();
            app.UseNexusLogRequestAndResponse();
            app.UseNexusExceptionToFulcrumResponse();
            app.UseNexusTokenValidationHandler(Configuration["Nexus:PublicKey"]);

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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mocks");
            });
        }

        private void ConfigureNexus(IServiceCollection services)
        {
            var tenant = new Tenant(Configuration["Nexus:Organization"], Configuration["Nexus:Environment"]);
            var credentials = new AuthenticationCredentials { ClientId = Configuration["Nexus:ClientId"], ClientSecret = Configuration["Nexus:ClientSecret"] };
            var nexusAuthManager = new NexusAuthenticationManager(tenant, Configuration["Nexus:FundamentalsUrl"]);
            var nexusTokenRefresher = nexusAuthManager.CreateTokenRefresher(credentials);

            var translatorService = CreateTranslatorService(tenant, nexusTokenRefresher);
            ValueTranslatorHttpSender.TranslatorService = translatorService;
            services.AddSingleton(provider => translatorService);

            services.AddValueTranslatorFilter();
            services.AddScoped(provider => new Capability1RestClient(new ValueTranslatorHttpSender(new HttpSender(Configuration["Capability1:BaseUrl"]), "capability1-mock")));
        }

        private ITranslatorService CreateTranslatorService(Tenant tenant, ITokenRefresherWithServiceClient nexusTokenRefresher)
        {
            var valueTranslatorUrl = Configuration["Nexus:ValueTranslatorUrl"];
            var translateClient = new TranslateClient(valueTranslatorUrl, tenant, nexusTokenRefresher.GetServiceClient());
            return new TranslatorService(translateClient);
        }
    }
}
