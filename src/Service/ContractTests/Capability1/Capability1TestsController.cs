using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.AspNet.Annotations;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Service.ContractTests.Mocks;
using Service.Controllers;
using Service.Mapping;
using Service.Models;
using SharedKernel;
#pragma warning disable 1591

namespace Service.ContractTests.Capability1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class Capability1TestsController : TestControllerBase, ITestable
    {
        private readonly ITestLogic _testLogic;
        private readonly Capability1RestClient _restclient;

        public Capability1TestsController(IConfiguration configuration, ITestLogic testLogic) : base(testLogic)
        {
            _testLogic = testLogic;

            var baseUri = $"{configuration["BaseUrl"]}/Capability1Mocks/api/v1/PersonManagement";
            _restclient = new Capability1RestClient(new HttpSender(baseUri));
        }

        public string Group => TestGrouping.CapabilityContractTests;

        [SwaggerGroup(TestGrouping.CapabilityContractTests)]
        [HttpPost("All")]
        public async Task<Test> RunAllAsync(Test parent = null)
        {
            var container = await _testLogic.CreateAsync("Capability 1 contract tests", parent);

            await RunTestablesSkippingRunAllAsync(container, new List<ControllerBase> { this });

            return container;
        }

        [SwaggerGroup(TestGrouping.CapabilityContractTests)]
        [HttpPost("Test1")]
        public async Task<Test> Test1(Test parent)
        {
            var test = await _testLogic.CreateAsync("Capability 1 Test 1 (event)", parent);

            FulcrumApplication.Context.CorrelationId = test.Id;

            // TODO: create entity in mock; mock publishes event via IntegrationTests service; intercept; set state
            // TODO: RestClient

            return test;
        }

        /// <summary>
        /// EXAMPLE: CRUD person entity
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        [SwaggerGroup(TestGrouping.CapabilityContractTests)]
        [HttpPost("CrudPerson")]
        public async Task<Test> CreatePerson(Test parent)
        {
            var test = await _testLogic.CreateAsync("CrudPerson", parent);

            try
            {
                // Create
                var person = await _restclient.CreatePerson(new MockPerson {Name = "Raginaharjar"});
                if (person?.Id == null) throw new Exception("No Person was created");
                var personId = person.Id;

                test.Properties = new Dictionary<string, object> { { "Person", person } };
                await _testLogic.UpdateAsync(test);

                // Read
                person = await _restclient.GetPerson(personId);
                if (person?.Id == null) throw new Exception($"Person {personId} could not be found");

                // TODO: Update

                // Delete
                await _restclient.DeletePerson(personId);
                person = await _restclient.GetPerson(personId);
                if (person != null) throw new Exception($"Person {personId} should be deleted");

                // All ok!
                await _testLogic.SetStateAsync(test, StateEnum.Ok, "ok");
            }
            catch (Exception e)
            {
                await _testLogic.SetStateAsync(test, StateEnum.Failed, e.Message);
            }


            return test;
        }
    }
}
