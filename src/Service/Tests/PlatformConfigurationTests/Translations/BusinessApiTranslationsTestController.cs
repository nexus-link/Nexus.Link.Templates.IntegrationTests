using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.RestClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nexus.Link.Authentication.Sdk;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.AspNet.Annotations;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Service.Configuration;
using Service.Controllers;
using Service.Logic;
using Service.Models;
using SharedKernel;

#pragma warning disable 1591

namespace Service.Tests.PlatformConfigurationTests.Translations
{
    /// <summary>
    /// Calls the business api and verifies that values are translated between clients
    /// </summary>
    [Authorize(AuthenticationSchemes = "Basic")]
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessApiTranslationsTestController : TestControllerBase, ITestable
    {
        private readonly BusinessApiRestClient _bapiClientAsCapability1Client;
        private readonly BusinessApiRestClient _bapiClientAsCapability2Client;

        public BusinessApiTranslationsTestController(IConfiguration configuration, ITestLogic testLogic) : base(configuration, testLogic)
        {
            var platformSettings = configuration.GetSection("Platform").Get<PlatformSettings>();

            var tokenRefresher1 = new TokenRefresher(configuration, configuration["Capability1:ClientId"], configuration["Capability1:ClientSecret"]);
            _bapiClientAsCapability1Client = new BusinessApiRestClient(new HttpSender(platformSettings.BusinessApiUrl, tokenRefresher1));

            var tokenRefresher2 = new TokenRefresher(configuration, configuration["Capability2:ClientId"], configuration["Capability2:ClientSecret"]);
            _bapiClientAsCapability2Client = new BusinessApiRestClient(new HttpSender(platformSettings.BusinessApiUrl, tokenRefresher2));
        }

        public string Group { get; } = TestGrouping.PlatformConfigurationTests;


        [SwaggerGroup(TestGrouping.PlatformConfigurationTests)]
        [HttpPost("All")]
        public async Task<Test> RunAllAsync(Test parent = null)
        {
            var container = await TestLogic.CreateAsync("Platform translations configuration tests", parent);

            await RunTestablesSkippingRunAllAsync(container, new List<ControllerBase> { this });

            return container;
        }

        /// <summary>
        /// Create an order as one client and get it as another
        /// </summary>
        [SwaggerGroup(TestGrouping.PlatformConfigurationTests)]
        [HttpPost("EnumTranslation")]
        public async Task<Test> EnumTranslation(Test parent)
        {
            var test = await TestLogic.CreateAsync("EnumTranslation", parent);
            var cap1Test = await TestLogic.CreateAsync("Capability 1", test);
            var cap2Test = await TestLogic.CreateAsync("Capability 2", test);

            try
            {
                // Create order as capability 1 client
                var createdOrder = await _bapiClientAsCapability1Client.CreateOrder(3, "Created");
                cap1Test.Properties = new Dictionary<string, object> { { "Order", createdOrder } };
                await TestLogic.UpdateAsync(cap1Test);
                if (createdOrder.Status != "Created") await TestLogic.SetStateAsync(cap1Test, StateEnum.Failed, "Expected Order.Status to be 'Created'");
                else await TestLogic.SetStateAsync(cap1Test, StateEnum.Ok, "Ok");

                // Fetch order as capability 2 client
                var order = await _bapiClientAsCapability2Client.GetOrder(createdOrder.Id);
                cap2Test.Properties = new Dictionary<string, object> { { "Order", order } };
                await TestLogic.UpdateAsync(cap2Test);
                if (order.Status != "New") await TestLogic.SetStateAsync(cap2Test, StateEnum.Failed, "Expected Order.Status to be 'New'");
                else await TestLogic.SetStateAsync(cap2Test, StateEnum.Ok, "Ok");
            }
            catch (Exception e)
            {
                await TestLogic.SetStateAsync(test, StateEnum.Failed, e.Message);
            }

            return test;
        }
    }
}
