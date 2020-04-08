using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Libraries.Web.AspNet.Annotations;
using Service.Controllers;
using Service.Mapping;
using Service.Models;
using SharedKernel;

namespace Service.ContractTests.Capability2
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class Capability2TestsController : TestControllerBase, ITestable
    {
        private readonly ITestLogic _testLogic;

        public Capability2TestsController(ITestLogic testLogic) : base(testLogic)
        {
            _testLogic = testLogic;
        }

        public string Group => TestGrouping.ContractTests;

        [SwaggerGroup(TestGrouping.ContractTests)]
        [HttpPost("All")]
        public async Task<Test> RunAllAsync(Test parent = null)
        {
            var container = await _testLogic.CreateAsync("Capability 2 contract tests", parent);

            await RunTestablesSkippingRunAllAsync(container, new List<ITestable> { this });

            return container;
        }

        [SwaggerGroup(TestGrouping.ContractTests)]
        [HttpPost("Test1")]
        public async Task<Test> Test1(Test parent)
        {
            var test = await _testLogic.CreateAsync("Capability 2 Test 1", parent);

            // TODO: Do test and update state
            await _testLogic.SetStateAsync(test, StateEnum.Failed, "awwwhh");

            return test;
        }

        [SwaggerGroup(TestGrouping.ContractTests)]
        [HttpPost("Test2")]
        public async Task<Test> Test2(Test parent)
        {
            var test = await _testLogic.CreateAsync("Capability Y Test 2", parent);

            // TODO: Do test and update state

            return test;
        }
    }
}
