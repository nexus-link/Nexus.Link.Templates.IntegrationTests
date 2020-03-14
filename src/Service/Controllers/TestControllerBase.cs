using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Libraries.Web.AspNet.Annotations;
using Service.Mapping;
using Service.Models;
using SharedKernel;

namespace Service.Controllers
{
    public abstract class TestControllerBase : ControllerBase
    {

        protected readonly ITestLogic TestLogic;

        protected TestControllerBase(ITestLogic testLogic)
        {
            TestLogic = testLogic;
        }

        protected List<ITestable> FindTestables(string group)
        {
            var testables = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof(ITestable).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x =>
                {
                    var instance = (ITestable)Activator.CreateInstance(x, TestLogic); // TODO: Not so nice
                    return instance;
                })
                .Where(x => x.Group == group)
                .ToList();

            return testables;
        }

        protected async Task RunTestables(Test container, List<ITestable> testables)
        {
            foreach (var testable in testables)
            {
                var methods = testable.GetType()
                    .GetMethods()
                    .Where(testMethod =>
                    {
                        var group = testMethod.GetCustomAttribute<SwaggerGroupAttribute>();
                        var hideInSwagger = testMethod.GetCustomAttribute<ApiExplorerSettingsAttribute>();
                        return group != null && hideInSwagger == null && testMethod.Name != nameof(ITestable.RunAllAsync);
                    })
                    .ToList();

                foreach (var method in methods)
                {
                    var task = (Task<Test>)method.Invoke(testable, new object?[] { container.Id });
                    await task;
                }
            }
        }

    }
}
