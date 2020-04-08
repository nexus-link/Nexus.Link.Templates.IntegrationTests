using System;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Web.AspNet.Annotations;
#pragma warning disable 1591

namespace Service.ContractTests.Mocks
{
    /// <summary>
    /// Used for mocking capability providers.
    /// </summary>
    /// <remarks>
    /// Only single instance support.
    /// </remarks>
    [Route("[controller]/api/v1/PersonManagement")]
    [ApiController]
    public class Capability1MocksController : ControllerBase
    {
        // Note! Assumes unique id among different entities
        private static readonly ConcurrentDictionary<string, dynamic> EntityStorage = new ConcurrentDictionary<string, dynamic>();

        [SwaggerGroup("Mocks")]
        [HttpPost("Persons")]
        public MockPerson CreatePerson(MockPerson person)
        {
            person.Id = Guid.NewGuid().ToString();
            EntityStorage.TryAdd(person.Id, person);
            return person;
        }

        [SwaggerGroup("Mocks")]
        [HttpGet("Persons/{id}")]
        public dynamic GetPersonById(string id)
        {
            if (!EntityStorage.ContainsKey(id)) throw new FulcrumNotFoundException();
            return EntityStorage[id];
        }

        [SwaggerGroup("Mocks")]
        [HttpDelete("Persons/{id}")]
        public void DeletePersonById(string id)
        {
            if (!EntityStorage.ContainsKey(id)) throw new FulcrumNotFoundException();
            EntityStorage.TryRemove(id, out _);
        }
    }

    public class MockPerson
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}