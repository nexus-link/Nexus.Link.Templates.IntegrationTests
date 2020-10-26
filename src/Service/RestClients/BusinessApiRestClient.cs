using System.Net;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Service.Tests.ContractTests.Capability1.Models;

#pragma warning disable 1591

namespace Service.RestClients
{
    public class BusinessApiRestClient : RestClient
    {
        public BusinessApiRestClient(IHttpSender httpSender) : base(httpSender)
        {
        }

        public async Task<MockOrder> CreateOrder(int items, string status)
        {
            var relativeUrl = "api/v1/BusinessApi/OrderManagement/Orders";
            var result = await PostAsync<MockOrder, MockOrder>(relativeUrl, new MockOrder
            {
                Items = items,
                Status = status
            });
            return result;
        }

        public async Task<MockOrder> GetOrder(string id)
        {
            var relativeUrl = $"api/v1/BusinessApi/OrderManagement/Orders/{WebUtility.UrlEncode(id)}";
            var result = await GetAsync<MockOrder>(relativeUrl);
            return result;
        }
    }
}
