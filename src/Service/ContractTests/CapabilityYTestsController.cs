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
    public class CapabilityYTestsController : TestControllerBase, ITestable
    {
        private readonly ITestLogic _testLogic;

        public CapabilityYTestsController(ITestLogic testLogic) : base(testLogic)
        {
            _testLogic = testLogic;
        }

        public string Group => SwaggerGroups.ContractTests;

        [SwaggerGroup(SwaggerGroups.ContractTests)]
        [HttpPost("All")]
        public async Task<Test> RunAllAsync(string parentId = null)
        {
            var container = await _testLogic.CreateAsync("Capability Y contract tests", parentId);

            await RunTestablesSkippingRunAllAsync(container, new List<ITestable> { this });

            await _testLogic.BuildTestTreeAsync(container);
            return container;
        }

        [SwaggerGroup(SwaggerGroups.ContractTests)]
        [HttpPost("Test1")]
        public async Task<Test> Test1(string parentId)
        {
            var test = await _testLogic.CreateAsync("Capability Y Test 1", parentId);

            // TODO: Do test and update state

            return test;
        }

        [SwaggerGroup(SwaggerGroups.ContractTests)]
        [HttpPost("Test2")]
        public async Task<Test> Test2(string parentId)
        {
            var test = await _testLogic.CreateAsync("Capability Y Test 2", parentId);

            // TODO: Do test and update state

            return test;
        }
    }
}
