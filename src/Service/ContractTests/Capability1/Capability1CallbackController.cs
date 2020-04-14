using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Service.ContractTests.Capability1.Models;
using Service.Controllers;
using Service.Logic;
using SharedKernel;
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

        public Capability1CallbackController(ITestLogic testLogic) : base(testLogic)
        {
        }

        [HttpPost("Test1")]
        public async Task Test1Callback(Event1 payload)
        {
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
