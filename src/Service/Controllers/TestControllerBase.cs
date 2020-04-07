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

        protected async Task RunTestablesSkippingRunAllAsync(Test container, List<ITestable> testables)
        {
            await RunTestablesOnlyRunAllAsync(container, testables, false);
        }

        protected async Task RunTestablesOnlyRunAllAsync(Test container, List<ITestable> testables)
        {
            await RunTestablesOnlyRunAllAsync(container, testables, true);
        }

        private async Task RunTestablesOnlyRunAllAsync(Test container, List<ITestable> testables, bool runAll)
        {
            foreach (var testable in testables)
            {
                var methods = testable.GetType()
                    .GetMethods()
                    .Where(testMethod =>
                    {
                        var group = testMethod.GetCustomAttribute<SwaggerGroupAttribute>();
                        var hideInSwagger = testMethod.GetCustomAttribute<ApiExplorerSettingsAttribute>();
                        return group != null && hideInSwagger == null &&
                                ((runAll && testMethod.Name == nameof(ITestable.RunAllAsync)) ||
                                (!runAll && testMethod.Name != nameof(ITestable.RunAllAsync)));
                    })
                    .ToList();

                foreach (var method in methods)
                {
                    Test test = null;
                    try
                    {
#pragma warning disable 8632
                        var task = (Task<Test>)method.Invoke(testable, new object?[] { container });
#pragma warning restore 8632
                        test = await task;
                    }
                    catch (Exception e)
                    {
                        if (test != null) await TestLogic.SetStateAsync(test, StateEnum.Failed, e.Message);
                        else await TestLogic.SetStateAsync(container, StateEnum.Failed, e.Message);
                    }
                }
            }
        }

    }
}
