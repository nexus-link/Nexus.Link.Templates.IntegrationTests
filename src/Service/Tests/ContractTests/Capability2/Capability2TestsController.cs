﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nexus.Link.Libraries.Web.AspNet.Annotations;
using Service.Configuration;
using Service.Controllers;
using Service.Logic;
using Service.Models;
using SharedKernel;

#pragma warning disable 1591

namespace Service.Tests.ContractTests.Capability2
{
    [Authorize(AuthenticationSchemes = "Basic")]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class Capability2TestsController : TestControllerBase, ITestable
    {
        public Capability2TestsController(IConfiguration configuration, ITestLogic testLogic) : base(configuration, testLogic)
        {
        }

        public string Group => TestGrouping.CapabilityContractTests;

        [SwaggerGroup(TestGrouping.CapabilityContractTests)]
        [HttpPost("All")]
        public async Task<Test> RunAllAsync(Test parent = null)
        {
            var container = await TestLogic.CreateAsync("Capability 2 contract tests", parent);

            await RunTestablesSkippingRunAllAsync(container, new List<ControllerBase> { this });

            return container;
        }

        // :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        // :::::::::::::::::: TASK: Setup the tests ::::::::::::::::::
        // :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

        [SwaggerGroup(TestGrouping.CapabilityContractTests)]
        [HttpPost("Test1")]
        public async Task<Test> Test1(Test parent)
        {
            var test = await TestLogic.CreateAsync("Capability 2 Test 1", parent);

            // TODO: Do test and update state
            await TestLogic.SetStateAsync(test, StateEnum.Ok, "Ohh yeah");

            return test;
        }

        [SwaggerGroup(TestGrouping.CapabilityContractTests)]
        [HttpPost("Test2")]
        public async Task<Test> Test2(Test parent)
        {
            var test = await TestLogic.CreateAsync("Capability Y Test 2", parent);

            // TODO: Do test and update state
            await TestLogic.SetStateAsync(test, StateEnum.Ok, "Done");

            return test;
        }
    }
}
