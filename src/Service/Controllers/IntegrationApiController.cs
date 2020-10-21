using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Authentication.Sdk;
using Nexus.Link.BusinessEvents.Sdk;
using Nexus.Link.BusinessEvents.Sdk.RestClients.Models;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Libraries.Web.AspNet.Annotations;
using Service.Configuration;
using Service.Logic;
using SharedKernel;

namespace Service.Controllers
{
    /// <summary>
    /// Fulfills the exact same contract as the integration part of the business api (Event sending, Authentication, etc.)
    ///
    /// Used to intercept events in the Capability contract tests
    /// </summary>
    // :::::::::::::::::::::::::::::::::::::::::::::::::::::::
    // :::::::::::::::::: TASK: Change role ::::::::::::::::::
    // :::::::::::::::::::::::::::::::::::::::::::::::::::::::
    [Authorize(Roles = "business-api-caller")]
    [Route("[controller]/api/v1")]
    [ApiController]
    public class IntegrationApiController : ControllerBase
    {

        private readonly ITestLogic _testLogic;
        private readonly IBusinessEvents _businessEventsClient;

        /// <summary></summary>
        public IntegrationApiController(IConfiguration configuration, ITestLogic testLogic)
        {
            _testLogic = testLogic;

            var nexusSettings = configuration.GetSection("Nexus").Get<NexusSettings>();
            var platformSettings = configuration.GetSection("Platform").Get<PlatformSettings>();
            // Note! Assumes same /Tokens endpoint in the Integration API as in Nexus Fundamentals
            var authManager = new NexusAuthenticationManager(nexusSettings.Tenant, platformSettings.IntegrationApiUrl);
            var tokenRefresher = authManager.CreateTokenRefresher(new AuthenticationCredentials { ClientId = platformSettings.ClientId, ClientSecret = platformSettings.ClientSecret });
            // Note! Assumes same /TestBench endpoint in the Integration API as in Nexus Business Events
            _businessEventsClient = new BusinessEvents(platformSettings.IntegrationApiUrl, nexusSettings.Tenant, tokenRefresher.GetServiceClient());
        }


        /// <summary>
        /// Captures the event and sends it to Nexus Business Events test bench
        /// </summary>
        [HttpPost]
        [Route("BusinessEvents/Publish/{entityName}/{eventName}/{major}/{minor}")]
        [SwaggerGroup("IntegrationCapability/BusinessEvents")]
        public async Task Publish(string entityName, string eventName, int major, int minor, dynamic content)
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
                    var result = (PublicationTestResult)await _businessEventsClient.TestBenchPublish(entityName, eventName, major, minor, client, payload);

                    if (result.Verified)
                    {
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