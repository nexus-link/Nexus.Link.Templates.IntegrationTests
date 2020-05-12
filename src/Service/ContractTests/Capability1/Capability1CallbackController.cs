using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Authentication.Sdk;
using Nexus.Link.BusinessEvents.Sdk;
using Nexus.Link.BusinessEvents.Sdk.RestClients.Models;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Service.ContractTests.Capability1.Models;
using Service.Controllers;
using Service.Logic;
using SharedKernel;
using JsonSerializer = System.Text.Json.JsonSerializer;

#pragma warning disable 1591

namespace Service.ContractTests.Capability1
{
    /// <summary>
    /// Represents a test strategy where the Platform Integration Test Service is a subscriber of events.
    ///
    /// (The other strategy is for the adapters to use the Platform Integration Test Service
    /// as their "Integration Api" which they use to send events, which are then intercepted)
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class Capability1CallbackController : TestControllerBase
    {
        private readonly IBusinessEvents _businessEventsClient;

        public Capability1CallbackController(IConfiguration configuration, ITestLogic testLogic) : base(testLogic)
        {
            var nexusSettings = configuration.GetSection("Nexus").Get<NexusSettings>();
            var platformSettings = configuration.GetSection("Platform").Get<PlatformSettings>();
            // Note! Assumes same /Tokens endpoint in the Business API as in Nexus Fundamentals
            var authManager = new NexusAuthenticationManager(nexusSettings.Tenant, platformSettings.BusinessApiUrl);
            var tokenRefresher = authManager.CreateTokenRefresher(new AuthenticationCredentials { ClientId = platformSettings.ClientId, ClientSecret = platformSettings.ClientSecret });
            // Note! Assumes same /TestBench endpoint in the Business API as in Nexus Business Events
            _businessEventsClient = new BusinessEvents(platformSettings.BusinessApiUrl, nexusSettings.Tenant, tokenRefresher.GetServiceClient());
        }

        [HttpPost("Test1")]
        public async Task Test1Callback(Event1 payload)
        {
            // Get the test
            var testId = FulcrumApplication.Context.CorrelationId;
            if (string.IsNullOrWhiteSpace(testId))
            {
                Log.LogError($"There was no correlation id. Payload was {JsonSerializer.Serialize(payload)}.");
                return;
            }

            var test = await TestLogic.GetAsync(testId);
            if (test == null)
            {
                Log.LogError($"There was no test with id {testId}. Payload was {JsonSerializer.Serialize(payload)}.");
                return;
            }

            // Check with the Nexus Business Events test bench
            var md = payload.MetaData;
            var result = await _businessEventsClient.TestBenchPublish(md.EntityName, md.EventName, md.MajorVersion, md.MinorVersion, md.PublisherName, JToken.FromObject(payload));

            if (result.Verified)
            {
                await TestLogic.SetStateAsync(test, StateEnum.Ok, "Event verified with BE Test bench");
            }
            else
            {
                var message = string.Join(", ", result.Errors) +
                              $" | Contract: {JsonConvert.SerializeObject(result.Contract)}" +
                              $" | Payload: {JsonConvert.SerializeObject(result.Payload)}";
                await TestLogic.SetStateAsync(test, StateEnum.Failed, message);
            }

            // Check translation
            if (payload?.Person?.Gender != "male")
            {
                await TestLogic.SetStateAsync(test, StateEnum.Failed, $"Expected {nameof(Event1.Person.Gender)} to be 'male', but was '{payload?.Person?.Gender}'");
            }
            else
            {
                await TestLogic.SetStateAsync(test, StateEnum.Ok, $"Event subscribed by {Startup.ApiName}");
            }

        }
    }
}
