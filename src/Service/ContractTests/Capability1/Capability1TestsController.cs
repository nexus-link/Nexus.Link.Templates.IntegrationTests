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
using Service.Logic;
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

            var baseUri = $"{configuration["BaseUrl"]}/Capability1Mocks/api/v1";
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

        /// <summary>
        /// EXAMPLE: Trigger event by creating entity
        /// </summary>
        [SwaggerGroup(TestGrouping.CapabilityContractTests)]
        [HttpPost("OrderCreatedEvent")]
        public async Task<Test> OrderCreatedEvent(Test parent)
        {
            var test = await _testLogic.CreateAsync("OrderCreatedEvent", parent);
            var createTest = await _testLogic.CreateAsync("Create", test);
            var eventTest = await _testLogic.CreateAsync("Event", test);

            try
            {
                FulcrumApplication.Context.CorrelationId = eventTest.Id;

                var order = await _restclient.CreateOrder(new MockOrder { Id = "1", Items = 3 });
                if (order?.Id == null) throw new Exception("No Order was created");

                createTest.Properties = new Dictionary<string, object> { { "Order", order } };
                await _testLogic.UpdateAsync(createTest);
                await _testLogic.SetStateAsync(createTest, StateEnum.Ok, "Order created");

            }
            catch (Exception e)
            {
                await _testLogic.SetStateAsync(test, StateEnum.Failed, e.Message);
            }

            // Note! We leave the state as Waiting and set to Ok once the event is intercepted in IntegrationApiController

            return test;
        }

        /// <summary>
        /// EXAMPLE: CRUD person entity
        /// </summary>
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
