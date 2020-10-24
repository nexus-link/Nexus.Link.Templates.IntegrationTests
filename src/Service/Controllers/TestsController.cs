using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Web.AspNet.Annotations;
using Service.Configuration;
using Service.Logic;
using SharedKernel;

namespace Service.Controllers
{
    /// <summary>
    /// Methods regarding specific instances of tests
    /// </summary>
    [Authorize(AuthenticationSchemes = "Basic")]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TestsController : ControllerBase
    {
        private readonly ITestLogic _testLogic;

        /// <summary></summary>
        public TestsController(ITestLogic testLogic)
        {
            _testLogic = testLogic;
        }

        /// <summary>
        /// Get a test by id
        /// </summary>
        [SwaggerGroup(TestGrouping.Common)]
        [HttpGet("{id}")]
        public async Task<Test> Get(Guid id)
        {
            var test = await _testLogic.GetAsync(id.ToString());
            if (test == null) throw new FulcrumNotFoundException(id.ToString());
            return test;
        }
    }
}
