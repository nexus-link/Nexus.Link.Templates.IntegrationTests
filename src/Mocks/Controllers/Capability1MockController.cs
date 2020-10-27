using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Mocks.Helpers;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Web.RestClientHelper;

#pragma warning disable 1591

namespace Mocks.Controllers
{
    /// <summary>
    /// Used for mocking capability providers.
    ///
    /// Testing pattern: "Platform integration test service as the Business API"
    /// https://www.lucidchart.com/publicSegments/view/38f444a9-dc67-40dc-b639-5054f6a42043/image.png
    ///
    /// When sending events, use the Platform Integration Tests service.
    /// It will check the event with Nexus Business Events test bench.
    /// </summary>
    /// <remarks>
    /// Only single instance support.
    /// </remarks>
    [AllowAnonymous]
    [Route("api/v1/Capability1Adapater")]
    [ApiController]
    public class Capability1MockController : ControllerBase
    {
        // Note! Assumes unique id among different entities
        private static readonly ConcurrentDictionary<string, dynamic> EntityStorage = new ConcurrentDictionary<string, dynamic>();

        private readonly IntegrationApiRestClient _integrationApiRestClient;

        public Capability1MockController(IConfiguration configuration)
        {
            // Use Platform integration test service as "integration api" (it will intercept events and run them through test bench)
            var baseUri = $"{configuration["PlatformTestService:BaseUrl"]}/api/v1/IntegrationApi";
            var tokenRefresher = new TokenRefresher(configuration);
            _integrationApiRestClient = new IntegrationApiRestClient(new HttpSender(baseUri, tokenRefresher));
        }

        [HttpPost("PersonManagement/Persons")]
        public MockPerson CreatePerson(MockPerson person)
        {
            person.Id = Guid.NewGuid().ToString();
            EntityStorage.TryAdd(person.Id, person);
            return person;
        }

        [HttpGet("PersonManagement/Persons/{id}")]
        public dynamic GetPersonById(string id)
        {
            if (!EntityStorage.ContainsKey(id)) throw new FulcrumNotFoundException();
            return EntityStorage[id];
        }

        [HttpDelete("PersonManagement/Persons/{id}")]
        public void DeletePersonById(string id)
        {
            if (!EntityStorage.ContainsKey(id)) throw new FulcrumNotFoundException();
            EntityStorage.TryRemove(id, out _);
        }

        [HttpPost("OrderManagement/Orders")]
        public async Task<MockOrder> CreateOrder(MockOrder order)
        {
            order.Id = Guid.NewGuid().ToString();
            EntityStorage.TryAdd(order.Id, order);

            await _integrationApiRestClient.PublishEvent("Order", "Created", 1, 0, "capability1-mock", JObject.FromObject(new MockOrderEvent { OrderId = order.Id, Items = order.Items, Status = "Created" }));

            return order;
        }

        [HttpGet("OrderManagement/Orders/{id}")]
        public dynamic GetOrder(string id)
        {
            if (!EntityStorage.ContainsKey(id)) throw new FulcrumNotFoundException();
            return EntityStorage[id];
        }
    }
}