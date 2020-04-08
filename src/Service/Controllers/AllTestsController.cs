using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Libraries.Web.AspNet.Annotations;
using Service.Mapping;
using SharedKernel;

namespace Service.Controllers
{
    /// <summary>
    /// Convenience for running all tests in different categories.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AllTestsController : TestControllerBase
    {
        /// <summary></summary>
        public AllTestsController(ITestLogic testLogic) : base(testLogic)
        {
        }

        /// <summary>
        /// Run ALL tests.
        /// </summary>
        /// <returns></returns>
        [SwaggerGroup(TestGrouping.Common)]
        [HttpPost]
        public async Task<Test> TopLevelAllTestsAsync()
        {
            var root = await TestLogic.CreateRootAsync("All");
            await RunTestablesSkippingRunAllAsync(root, new List<ControllerBase> { this });

            return root;
        }

        /// <summary>
        /// Run all Capability contract tests
        /// </summary>
        /// <param name="parent">The parent test. You would almost certainly set this to null.</param>
        [SwaggerGroup(TestGrouping.Common)]
        [HttpPost("AllCapabilityContractTests")]
        public async Task<Test> CapabilityContractTests(Test parent = null)
        {
            return await RunTopLevelTestAsync(parent, TestGrouping.CapabilityContractTests);
        }

        /// <summary>
        /// Run all Configuration tests
        /// </summary>
        /// <param name="parent">The parent test. You would almost certainly set this to null.</param>
        [SwaggerGroup(TestGrouping.Common)]
        [HttpPost("AllConfigurationTests")]
        public async Task<Test> ConfigurationTests(Test parent = null)
        {
            return await RunTopLevelTestAsync(parent, TestGrouping.ConfigurationTests);
        }

        private async Task<Test> RunTopLevelTestAsync(Test parent, string group)
        {
            var container = await TestLogic.CreateAsync(group, parent);

            try
            {
                var testables = FindTestables(group);
                await RunTestablesOnlyRunAllAsync(container, testables);
            }
            catch (Exception e)
            {
                await TestLogic.SetStateAsync(container, StateEnum.Failed, e.Message);
            }

            return container;
        }
    }
}
