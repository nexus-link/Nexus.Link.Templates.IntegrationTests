using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Nexus.Link.Authentication.Sdk;
using Nexus.Link.BusinessEvents.Sdk;
using Nexus.Link.BusinessEvents.Sdk.RestClients.Models;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Authentication;

namespace Mocks.Controllers
{
    /// <summary>
    /// Pretends to be the customer's "Integration API", so that the tests in template project can be run.
    /// </summary>
    [Route("api/v1")]
    [ApiController]
    public class IntegrationApiMockController : ControllerBase
    {
        private readonly IBusinessEvents _businessEventsClient;
        private readonly AuthenticationManager _authManager;

        public IntegrationApiMockController(IConfiguration configuration)
        {
            var tenant = new Tenant(configuration["Nexus:Organization"], configuration["Nexus:Environment"]);
            _authManager = new AuthenticationManager(tenant, configuration["Nexus:FundamentalsUrl"]);

            var credentials = new AuthenticationCredentials { ClientId = configuration["Nexus:ClientId"], ClientSecret = configuration["Nexus:ClientSecret"] };
            var nexusAuthManager = new NexusAuthenticationManager(tenant, configuration["Nexus:FundamentalsUrl"]);
            _businessEventsClient = new BusinessEvents(configuration["Nexus:BusinessEventsUrl"], tenant, nexusAuthManager.CreateTokenRefresher(credentials).GetServiceClient());
        }

        [HttpPost("Authentication/Tokens")]
        public async Task<AuthenticationToken> CreateTokenAsync(AuthenticationCredentials tokenCredentials)
        {
            return await _authManager.GetJwtTokenAsync(tokenCredentials);
        }

        [HttpPost("TestBench/{entityName}/{eventName}/{major}/{minor}/Publish")]
        public async Task<PublicationTestResult> TestBenchPublish(string entityName, string eventName, int major, int minor, string clientName, JObject payload)
        {
            var result = await _businessEventsClient.TestBenchPublish(entityName, eventName, major, minor, clientName, payload);
            return result;
        }

    }
}
