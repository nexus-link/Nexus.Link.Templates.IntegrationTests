using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.BusinessEvents.Sdk.RestClients.Models;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Libraries.Web.AspNet.Annotations;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Service.Configuration;
using Service.Logic;
using Service.RestClients;
using SharedKernel;
#pragma warning disable 1591

namespace Service.Controllers
{
    /// <summary>
    /// Fulfills the exact same contract as the integration part of the business api (Event sending, Authentication, etc.)
    ///
    /// Used to intercept events in the Capability contract tests.
    /// Let the capability provider services use this service as the "Integration API" for sending events.
    /// </summary>
    // :::::::::::::::::::::::::::::::::::::::::::::::::::::::
    // :::::::::::::::::: TASK: Change role ::::::::::::::::::
    // :::::::::::::::::::::::::::::::::::::::::::::::::::::::
    [Authorize(Roles = "business-api-caller")]
    [Route("api/v1/IntegrationApi")]
    [ApiController]
    public class IntegrationApiController : ControllerBase
    {
        private readonly ITestLogic _testLogic;
        private readonly IntegrationApiRestClient _integrationApiClient;

        /// <summary></summary>
        public IntegrationApiController(IConfiguration configuration, ITestLogic testLogic)
        {
            _testLogic = testLogic;

            var platformSettings = configuration.GetSection("Platform").Get<PlatformSettings>();
            var tokenRefresher = new TokenRefresher(configuration, platformSettings.ClientId, platformSettings.ClientSecret);
            _integrationApiClient = new IntegrationApiRestClient(new HttpSender(platformSettings.IntegrationApiUrl, tokenRefresher.GetServiceClient()));
        }


        [HttpPost("Authentication/Tokens")]
        public async Task<AuthenticationToken> CreateTokenAsync(AuthenticationCredentials tokenCredentials)
        {
            var result = await _integrationApiClient.CreateToken(tokenCredentials.ClientId, tokenCredentials.ClientSecret);
            return result;
        }


        /// <summary>
        /// Captures the event and sends it to Nexus Business Events test bench for verification
        /// </summary>
        [HttpPost("BusinessEvents/Publish/{entityName}/{eventName}/{major}/{minor}")]
        [SwaggerGroup("IntegrationCapability/BusinessEvents")]
        public async Task Publish(string entityName, string eventName, int major, int minor, JObject content)
        {
            ServiceContract.RequireNotNull(entityName, nameof(entityName));
            ServiceContract.RequireNotNull(eventName, nameof(eventName));
            ServiceContract.RequireGreaterThan(0, major, nameof(major));
            ServiceContract.RequireGreaterThanOrEqualTo(0, minor, nameof(minor));
            ServiceContract.RequireNotNull(content, nameof(content));

            var correlationId = FulcrumApplication.Context.CorrelationId;
            var test = await _testLogic.GetAsync(correlationId);
            if (test != null)
            {
                try
                {
                    var client = FulcrumApplication.Context.CallingClientName;
                    var payload = JObject.FromObject(content);
                    var result = await _integrationApiClient.TestBenchPublish(entityName, eventName, major, minor, client, payload);

                    if (result.Verified)
                    {
                        await _integrationApiClient.PublishEvent(entityName, eventName, major, minor, client, payload);
                        await _testLogic.SetStateAsync(test, StateEnum.Ok, "Event intercepted and verified with BE Test bench");
                    }
                    else
                    {
                        var message = string.Join(", ", result.Errors) +
                                      $" | Contract: {JsonConvert.SerializeObject(result.Contract)}" +
                                      $" | Payload: {JsonConvert.SerializeObject(result.Payload)}";
                        await _testLogic.SetStateAsync(test, StateEnum.Failed, message);
                    }

                }
                catch (Exception e)
                {
                    await _testLogic.SetStateAsync(test, StateEnum.Failed, e.Message);
                }
            }
        }
    }
}