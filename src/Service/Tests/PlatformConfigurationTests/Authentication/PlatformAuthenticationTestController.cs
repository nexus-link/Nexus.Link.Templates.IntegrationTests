﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.RestClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.AspNet.Annotations;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Service.Configuration;
using Service.Controllers;
using Service.Logic;
using Service.Models;
using SharedKernel;

#pragma warning disable 1591

namespace Service.Tests.PlatformConfigurationTests.Authentication
{
    /// <summary>
    /// Tests Nexus Authentication as a service in the platform
    /// </summary>
    [Authorize(AuthenticationSchemes = "Basic")]
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformAuthenticationTestController : TestControllerBase, ITestable
    {
        private readonly IntegrationApiRestClient _apiRestClient;
        private readonly PlatformSettings _platformSettings;

        /// <summary></summary>
        public PlatformAuthenticationTestController(IConfiguration configuration, ITestLogic testLogic) : base(configuration, testLogic)
        {
            _platformSettings = configuration.GetSection("Platform").Get<PlatformSettings>();
            _apiRestClient = new IntegrationApiRestClient(new HttpSender(_platformSettings.IntegrationApiUrl));
        }


        public string Group { get; } = TestGrouping.PlatformConfigurationTests;

        [SwaggerGroup(TestGrouping.PlatformConfigurationTests)]
        [HttpPost("All")]
        public async Task<Test> RunAllAsync(Test parent = null)
        {
            var container = await TestLogic.CreateAsync("Platform authentication configuration tests", parent);

            await RunTestablesSkippingRunAllAsync(container, new List<ControllerBase> { this });

            return container;
        }

        /// <summary>
        /// Create a JWT through the Business API for known API user
        /// </summary>
        [SwaggerGroup(TestGrouping.PlatformConfigurationTests)]
        [HttpPost("AuthenticatePlatformClient")]
        public async Task<Test> AuthenticatePlatformClient(Test parent)
        {
            var test = await TestLogic.CreateAsync("AuthenticatePlatformClient", parent);

            try
            {
                FulcrumApplication.Context.CorrelationId = test.Id;

                var result = await _apiRestClient.CreateToken(_platformSettings.ClientId, _platformSettings.ClientSecret);
                if (string.IsNullOrWhiteSpace(result?.AccessToken))
                {
                    await TestLogic.SetStateAsync(test, StateEnum.Failed, $"Could not authenticate client '{_platformSettings.ClientId}");
                }
                else
                {
                    await TestLogic.SetStateAsync(test, StateEnum.Ok, "Ok");
                }
            }
            catch (Exception e)
            {
                await TestLogic.SetStateAsync(test, StateEnum.Failed, e.Message);
            }

            return test;
        }

    }
}