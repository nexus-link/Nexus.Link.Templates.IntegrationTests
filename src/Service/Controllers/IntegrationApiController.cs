using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nexus.Link.BusinessEvents.Sdk;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.AspNet.Annotations;
using Service.Logic;
using SharedKernel;

namespace Service.Controllers
{
    /// <summary>
    /// Fulfills the exact same contract as the integration part of the business api (Event sending, Authentication, etc.)
    ///
    /// Used to intercept events in the Capability contract tests
    /// </summary>
    [Route("[controller]/api/v1")]
    [ApiController]
    public class IntegrationApiController : ControllerBase
    {

        private readonly ITestLogic _testLogic;
        private readonly Nexus.Link.BusinessEvents.Sdk.IBusinessEvents _businessEventsClients;

        /// <summary></summary>
        public IntegrationApiController(IConfiguration configuration, ITestLogic testLogic)
        {
            _testLogic = testLogic;

            //var nexusSettings = configuration.GetSection("Nexus");
            //_businessEventsClients = new BusinessEvents(nexusSettings["BusinessEventsUrl"], );
        }


        /// <summary>
        /// Mocks publishing an event
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
                // TODO: Send event to test bench in Nexus Business Events

                await _testLogic.SetStateAsync(test, StateEnum.Ok, "Event intercepted");
            }
        }
    }
}