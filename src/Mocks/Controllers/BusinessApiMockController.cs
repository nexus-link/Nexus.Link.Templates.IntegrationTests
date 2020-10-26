using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mocks.Helpers;
using Newtonsoft.Json.Linq;

namespace Mocks.Controllers
{
    [Route("api/v1/BusinessApi")]
    [ApiController]
    public class BusinessApiMockController : ControllerBase
    {
        [HttpPost("OrderManagement/Orders")]
        public async Task<MockOrder> CreateOrder(MockOrder order)
        {
            // TODO

            return order;
        }

        [HttpPost("OrderManagement/Orders/{id}")]
        public async Task<MockOrder> GetOrder(string id)
        {
            // TODO

            return new MockOrder { Id = id };
        }
    }
}
