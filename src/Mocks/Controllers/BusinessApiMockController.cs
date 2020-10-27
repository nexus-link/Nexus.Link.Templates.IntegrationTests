using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mocks.Helpers;

namespace Mocks.Controllers
{
    [Route("api/v1/BusinessApi")]
    [ApiController]
    public class BusinessApiMockController : ControllerBase
    {
        private readonly Capability1RestClient _capability1RestClient;

        public BusinessApiMockController(Capability1RestClient capability1RestClient)
        {
            _capability1RestClient = capability1RestClient;
        }

        [HttpPost("OrderManagement/Orders")]
        public async Task<MockOrder> CreateOrder(MockOrder order)
        {
            var result = await _capability1RestClient.CreateOrder(order);
            return result;
        }

        [HttpGet("OrderManagement/Orders/{id}")]
        public async Task<MockOrder> GetOrder(string id)
        {
            var result = await _capability1RestClient.GetOrder(id);
            return result;
        }
    }
}
