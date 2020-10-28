﻿using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Dashboard.Hubs;
using Dashboard.Logic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Dashboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestsController : ControllerBase
    {
        private readonly IHubContext<TestsHub> _hubContext;
        private readonly ITestLogic _testLogic;

        public TestsController(IHubContext<TestsHub> hubContext, ITestLogic testLogic)
        {
            _hubContext = hubContext;
            _testLogic = testLogic;
        }


        [HttpPost("Start")]
        [Produces("text/plain")]
        public async Task<string> StartTest()
        {
            var test = await _testLogic.CreateTest();
            await _hubContext.Clients.All.SendAsync("TestChanged", test);
            return test.GetProperty("Id").GetString();
        }

    }
}