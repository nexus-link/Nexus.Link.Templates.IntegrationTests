using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nexus.Link.Libraries.Web.AspNet.Annotations;
using Service.Logic;
using Service.Models;
using SharedKernel;

namespace Service.Controllers
{
    /// <summary></summary>
    public abstract class TestControllerBase : ControllerBase
    {
        /// <summary></summary>
        protected readonly IConfiguration Configuration;
        /// <summary></summary>
        protected readonly ITestLogic TestLogic;

        /// <summary></summary>
        protected TestControllerBase(IConfiguration configuration, ITestLogic testLogic)
        {
            Configuration = configuration;
            TestLogic = testLogic;
        }

        /// <summary></summary>
        protected List<ControllerBase> FindTestables(string group)
        {
            var testables = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof(ITestable).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x =>
                {
                    // TODO: Would be nice with dependency injection
                    var instance = (ITestable)Activator.CreateInstance(x, Configuration, TestLogic); // TODO: Not so nice
                    return instance;
                })
                .Where(x => x.Group == group)
                .Select(x => (ControllerBase)x)
                .ToList();

            return testables;
        }

        /// <summary></summary>
        protected async Task RunTestablesSkippingRunAllAsync(Test container, List<ControllerBase> testables)
        {
            await RunTestablesOnlyRunAllAsync(container, testables, false);
        }

        /// <summary></summary>
        protected async Task RunTestablesOnlyRunAllAsync(Test container, List<ControllerBase> testables)
        {
            await RunTestablesOnlyRunAllAsync(container, testables, true);
        }

        private async Task RunTestablesOnlyRunAllAsync(Test container, List<ControllerBase> testables, bool runAll)
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
                                (!runAll && testMethod.Name != nameof(ITestable.RunAllAsync))) &&
                                testMethod.Name != "TopLevelAllTestsAsync"; // Special for preventing loops in AllTestsController
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
