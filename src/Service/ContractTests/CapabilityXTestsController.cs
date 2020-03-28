using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Libraries.Web.AspNet.Annotations;
using Service.Controllers;
using Service.Mapping;
using Service.Models;
using SharedKernel;

namespace Service.ContractTests
{
    [ApiController]
    [Route("[controller]")]
    public class CapabilityXTestsController : TestControllerBase, ITestable
    {
        private readonly ITestLogic _testLogic;

        public CapabilityXTestsController(ITestLogic testLogic) : base(testLogic)
        {
            _testLogic = testLogic;
        }

        public string Group => SwaggerGroups.ContractTests;

        [SwaggerGroup(SwaggerGroups.ContractTests)]
        [HttpPost("All")]
        public async Task<Test> RunAllAsync(Test parent = null)
        {
            var container = await _testLogic.CreateAsync("Capability X contract tests", parent);

            await RunTestablesSkippingRunAllAsync(container, new List<ITestable> { this });

            return container;
        }

        [SwaggerGroup(SwaggerGroups.ContractTests)]
        [HttpPost("Test1")]
        public async Task<Test> Test1(Test parent)
        {
            var test = await _testLogic.CreateAsync("Capability X Test 1", parent);

            // TODO: Do test and update state

            await _testLogic.SetState(test, StateEnum.Ok, "ok");

            return test;
        }

        [SwaggerGroup(SwaggerGroups.ContractTests)]
        [HttpPost("Test2")]
        public async Task<Test> Test2(Test parent)
        {
            var test = await _testLogic.CreateAsync("Capability X Test 2", parent);

            // TODO: Do test and update state

            return test;
        }
    }
}
