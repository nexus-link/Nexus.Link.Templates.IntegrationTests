using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Web.AspNet.Annotations;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Service.RestClients;

#pragma warning disable 1591

namespace Service.Tests.ContractTests.Mocks
{
    /// <summary>
    /// Used for mocking capability providers.
    /// </summary>
    /// <remarks>
    /// Only single instance support.
    /// </remarks>
    // :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    // :::::::::::::::::: TASK: Remove this class ::::::::::::::::::
    // :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("[controller]/api/v1")]
    [ApiController]
    public class Capability1MocksController : ControllerBase
    {
        // Note! Assumes unique id among different entities
        private static readonly ConcurrentDictionary<string, dynamic> EntityStorage = new ConcurrentDictionary<string, dynamic>();

        private readonly IntegrationApiRestClient _apiRestClient;

        public Capability1MocksController(IConfiguration configuration)
        {
            var baseUri = $"{configuration["Service:BaseUrl"]}/IntegrationApi/api/v1"; // Use Platform integration test service as "integration api"
            _apiRestClient = new IntegrationApiRestClient(new HttpSender(baseUri));
        }

        [SwaggerGroup("Mocks")]
        [HttpPost("PersonManagement/Persons")]
        public MockPerson CreatePerson(MockPerson person)
        {
            person.Id = Guid.NewGuid().ToString();
            EntityStorage.TryAdd(person.Id, person);
            return person;
        }

        [SwaggerGroup("Mocks")]
        [HttpGet("PersonManagement/Persons/{id}")]
        public dynamic GetPersonById(string id)
        {
            if (!EntityStorage.ContainsKey(id)) throw new FulcrumNotFoundException();
            return EntityStorage[id];
        }

        [SwaggerGroup("Mocks")]
        [HttpDelete("PersonManagement/Persons/{id}")]
        public void DeletePersonById(string id)
        {
            if (!EntityStorage.ContainsKey(id)) throw new FulcrumNotFoundException();
            EntityStorage.TryRemove(id, out _);
        }

        [SwaggerGroup("Mocks")]
        [HttpPost("OrderManagement/Orders")]
        public async Task<MockOrder> CreateOrder(MockOrder order)
        {
            order.Id = Guid.NewGuid().ToString();
            EntityStorage.TryAdd(order.Id, order);

            await _apiRestClient.PublishEvent("Order", "Created", 1, 0, JObject.FromObject(new MockOrderEvent { OrderId = order.Id, Items = order.Items }));

            return order;
        }
    }

    public class MockPerson
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class MockOrder
    {
        public string Id { get; set; }
        public int Items { get; set; }
    }

    public class MockOrderEvent
    {
        public string OrderId { get; set; }
        public int Items { get; set; }
    }
}