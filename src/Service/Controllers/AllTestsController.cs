using System;
using System.Collections.Generic;
using System.Linq;
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
            //var x1 = await _testLogic.CreateAsync("A1", root);
            //var x2 = await _testLogic.CreateAsync("A2", root);
            //var y1 = await _testLogic.CreateAsync("B1", x1);
            try
            {
                var testables = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(x => x.GetTypes())
                    .Where(x => typeof(ITopLevel).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Select(x =>
                    {
                        var instance = (ITestable) Activator.CreateInstance(x, _testLogic); // TODO: Not so nice
                        return instance;
                    })
                    .ToList();

                await RunTestables(root, testables);
            }
            catch (Exception e)
            {
                // TODO
                throw;
                //testContext.Fail($"One of the tests did not catch the following exception: {e.ToLogString()}");
            }


            await _testLogic.BuildTestTreeAsync(root);
            return root;
        }

        private async Task RunTestables(Test root, List<ITestable> testables)
        {
            foreach (var testable in testables)
            {
                await testable.RunAllAsync(root.Id);
            }
        }
    }
}
