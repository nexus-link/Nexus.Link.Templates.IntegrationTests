using System.Net;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Web.RestClientHelper;

#pragma warning disable 1591

namespace Mocks.Helpers
{
    public class Capability1RestClient : RestClient
    {
        public Capability1RestClient(IHttpSender httpSender) : base(httpSender)
        {
        }

        public async Task<MockOrder> CreateOrder(MockOrder order)
        {
            const string relativeUrl = "OrderManagement/Orders";
            var result = await PostAsync<MockOrder, MockOrder>(relativeUrl, order);
            return result;
        }

        public async Task<MockOrder> GetOrder(string id)
        {
            var relativeUrl = $"OrderManagement/Orders/{WebUtility.UrlEncode(id)}";
            var result = await GetAsync<MockOrder>(relativeUrl);
            return result;
        }
    }
}
