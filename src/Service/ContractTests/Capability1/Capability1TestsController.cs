﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.AspNet.Annotations;
using Service.Controllers;
using Service.Mapping;
using Service.Models;
using SharedKernel;

namespace Service.ContractTests.Capability1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class Capability1TestsController : TestControllerBase, ITestable
    {
        private readonly ITestLogic _testLogic;

        public Capability1TestsController(ITestLogic testLogic) : base(testLogic)
        {
            _testLogic = testLogic;
        }

        public string Group => SwaggerGroups.ContractTests;

        [SwaggerGroup(SwaggerGroups.ContractTests)]
        [HttpPost("All")]
        public async Task<Test> RunAllAsync(Test parent = null)
        {
            var container = await _testLogic.CreateAsync("Capability 1 contract tests", parent);

            await RunTestablesSkippingRunAllAsync(container, new List<ITestable> { this });

            return container;
        }

        [SwaggerGroup(SwaggerGroups.ContractTests)]
        [HttpPost("Test1")]
        public async Task<Test> Test1(Test parent)
        {
            var test = await _testLogic.CreateAsync("Capability 1 Test 1 (event)", parent);

            FulcrumApplication.Context.CorrelationId = test.Id;
            // TODO: "Do call to capability and expect an event to be sent"


            return test;
        }

        [SwaggerGroup(SwaggerGroups.ContractTests)]
        [HttpPost("Test2")]
        public async Task<Test> Test2(Test parent)
        {
            var test = await _testLogic.CreateAsync("Capability 1 Test 2", parent);

            // TODO: Do test and update state
            await _testLogic.SetState(test, StateEnum.Ok, "ok");

            return test;
        }
    }
}
