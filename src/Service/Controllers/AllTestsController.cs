using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Web.AspNet.Annotations;
using Service.Mapping;
using Service.Models;
using SharedKernel;

namespace Service.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AllTestsController : ControllerBase
    {
        private readonly ITestLogic _testLogic;

        public AllTestsController(ITestLogic testLogic)
        {
            _testLogic = testLogic;
        }

        [SwaggerGroup(SwaggerGroups.Common)]
        [HttpPost]
        public async Task<Test> RunAllAsync()
        {
            var root = await _testLogic.CreateRootAsync("All");
            try
            {
                var testables = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(x => x.GetTypes())
                    .Where(x => typeof(ITopLevel).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Select(x =>
                    {
                        var instance = (ITestable)Activator.CreateInstance(x, _testLogic); // TODO: Not so nice
                        return instance;
                    })
                    .ToList();

                await RunTestables(root, testables);
            }
            catch (Exception e)
            {
                await _testLogic.SetStateAsync(root, StateEnum.Failed, e.Message);
            }

            return root;
        }

        private async Task RunTestables(Test root, List<ITestable> testables)
        {
            foreach (var testable in testables)
            {
                await testable.RunAllAsync(root);
            }
        }

        [SwaggerGroup(SwaggerGroups.Common)]
        [HttpGet("{id}")]
        public async Task<Test> Get(Guid id)
        {
            var test = await _testLogic.GetAsync(id.ToString());
            if (test == null) throw new FulcrumNotFoundException(id.ToString());
            return test;
        }
    }
}
