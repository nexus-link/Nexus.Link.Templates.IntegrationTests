using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.AspNet.Annotations;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Service.Configuration;
using Service.Controllers;
using Service.Logic;
using Service.Models;
using Service.Tests.ContractTests.Capability1.Models;
using SharedKernel;

#pragma warning disable 1591

namespace Service.Tests.ContractTests.Capability1
{
    [Authorize(AuthenticationSchemes = "Basic")]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class Capability1TestsController : TestControllerBase, ITestable
    {
        private readonly Capability1RestClient _restclient;

        public Capability1TestsController(IConfiguration configuration, ITestLogic testLogic) : base(configuration, testLogic)
        {
            var baseUri = $"{configuration["Capability1:BaseUrl"]}";
            _restclient = new Capability1RestClient(new HttpSender(baseUri));
        }

        public string Group => TestGrouping.CapabilityContractTests;

        [SwaggerGroup(TestGrouping.CapabilityContractTests)]
        [HttpPost("All")]
        public async Task<Test> RunAllAsync(Test parent = null)
        {
            var container = await TestLogic.CreateAsync("Capability 1 contract tests", parent);

            await RunTestablesSkippingRunAllAsync(container, new List<ControllerBase> { this });

            return container;
        }

        // :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        // :::::::::::::::::: TASK: Setup the tests ::::::::::::::::::
        // :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::


        /// <summary>
        /// EXAMPLE: Trigger event by creating entity
        /// </summary>
        [SwaggerGroup(TestGrouping.CapabilityContractTests)]
        [HttpPost("OrderCreatedEvent")]
        public async Task<Test> OrderCreatedEvent(Test parent)
        {
            var test = await TestLogic.CreateAsync("OrderCreatedEvent", parent);
            var createTest = await TestLogic.CreateAsync("Create", test);
            var eventTest = await TestLogic.CreateAsync("Event", test);

            try
            {
                FulcrumApplication.Context.CorrelationId = eventTest.Id;

                var order = await _restclient.CreateOrder(new MockOrder { Id = "1", Items = 3 });
                if (order?.Id == null) throw new Exception("No Order was created");

                createTest.Properties = new Dictionary<string, object> { { "Order", order } };
                await TestLogic.UpdateAsync(createTest);
                await TestLogic.SetStateAsync(createTest, StateEnum.Ok, "Order created");

            }
            catch (Exception e)
            {
                await TestLogic.SetStateAsync(test, StateEnum.Failed, e.Message);
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
            var test = await TestLogic.CreateAsync("CrudPerson", parent);

            try
            {
                // Create
                var person = await _restclient.CreatePerson(new MockPerson { Name = "Raginaharjar" });
                if (person?.Id == null) throw new Exception("No Person was created");
                var personId = person.Id;

                test.Properties = new Dictionary<string, object> { { "Person", person } };
                await TestLogic.UpdateAsync(test);

                // Read
                person = await _restclient.GetPerson(personId);
                if (person?.Id == null) throw new Exception($"Person {personId} could not be found");

                // TODO: Update

                // Delete
                await _restclient.DeletePerson(personId);
                person = await _restclient.GetPerson(personId);
                if (person != null) throw new Exception($"Person {personId} should be deleted");

                // All ok!
                await TestLogic.SetStateAsync(test, StateEnum.Ok, "ok");
            }
            catch (Exception e)
            {
                await TestLogic.SetStateAsync(test, StateEnum.Failed, e.Message);
            }


            return test;
        }
    }
}
