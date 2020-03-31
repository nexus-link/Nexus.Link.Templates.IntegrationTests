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
    [Route("api/v1/[controller]")]
    public class AllContractTests : TestControllerBase, ITestable, ITopLevel
    {

        public AllContractTests(ITestLogic testLogic) : base(testLogic)
        {
        }

        public string Group => SwaggerGroups.Common;

        [SwaggerGroup(SwaggerGroups.Common)]
        [HttpPost]
        public async Task<Test> RunAllAsync(Test parent = null)
        {
            var container = await TestLogic.CreateAsync("Contract tests", parent);

            try
            {
                var testables = FindTestables(SwaggerGroups.ContractTests);
                await RunTestablesOnlyRunAllAsync(container, testables);
            }
            catch (Exception e)
            {
                await TestLogic.SetState(container, StateEnum.Failed, e.Message);
            }

            return container;
        }

    }
}
