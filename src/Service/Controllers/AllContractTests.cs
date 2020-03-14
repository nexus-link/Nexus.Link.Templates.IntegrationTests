using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Libraries.Web.AspNet.Annotations;
using Service.Mapping;
using Service.Models;
using SharedKernel;

namespace Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AllContractTests : TestControllerBase, ITestable, ITopLevel
    {

        public AllContractTests(ITestLogic testLogic) : base(testLogic)
        {
        }

        public string Group => SwaggerGroups.Common;

        [SwaggerGroup(SwaggerGroups.Common)]
        [HttpPost]
        public async Task<Test> RunAllAsync(string parentId = null)
        {
            var container = await TestLogic.CreateAsync("Contract tests", parentId);

            try
            {
                var testables = FindTestables(SwaggerGroups.ContractTests);
                await RunTestablesOnlyRunAllAsync(container, testables);
            }
            catch (Exception e)
            {
                // TODO
                throw;
                //testContext.Fail($"One of the tests did not catch the following exception: {e.ToLogString()}");
            }

            await TestLogic.BuildTestTreeAsync(container);
            return container;
        }

    }
}
