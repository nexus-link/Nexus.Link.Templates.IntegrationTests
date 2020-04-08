using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.AspNet.Annotations;
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

        public Capability1TestsController(ITestLogic testLogic) : base(testLogic)
        {
            _testLogic = testLogic;
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
            // TODO: "Do call to capability and expect an event to be sent"


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
                using var httpClient = new HttpClient();
                
                // Create
                var url = $"{Request.Scheme}://{Request.Host}/Capability1Mocks/api/v1/PersonManagement/Persons";
                var response = await httpClient.PostAsJsonAsync(url, new MockPerson { Name = "Raginaharjar" });
                var result = await response.Content.ReadAsStringAsync();
                var person = System.Text.Json.JsonSerializer.Deserialize<MockPerson>(result);
                if (person?.Id == null) throw new Exception("No Person was created");
                var personId = person.Id;

                test.Properties = new Dictionary<string, object> { { "Person", person } };
                await _testLogic.UpdateAsync(test);

                // Read
                person = await GetMockPerson(personId, httpClient);
                if (person?.Id == null) throw new Exception($"Person {personId} could not be found");

                // TODO: Update

                // Delete
                var idUrl = $"{Request.Scheme}://{Request.Host}/Capability1Mocks/api/v1/PersonManagement/Persons/{personId}";
                response = await httpClient.DeleteAsync(idUrl);
                if (!response.IsSuccessStatusCode) throw new Exception($"Could not delete Person {personId}");

                // Deleted
                person = await GetMockPerson(personId, httpClient);
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

        private async Task<MockPerson> GetMockPerson(string personId, HttpClient httpClient)
        {
            var idUrl = $"{Request.Scheme}://{Request.Host}/Capability1Mocks/api/v1/PersonManagement/Persons/{personId}";
            var response = await httpClient.GetAsync(idUrl);
            if (!response.IsSuccessStatusCode) return null;
            var result = await response.Content.ReadAsStringAsync();
            var person = System.Text.Json.JsonSerializer.Deserialize<MockPerson>(result);
            return person;
        }
    }
}
