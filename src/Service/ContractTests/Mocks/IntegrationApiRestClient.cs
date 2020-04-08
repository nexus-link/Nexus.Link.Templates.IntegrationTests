using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Web.RestClientHelper;

#pragma warning disable 1591

namespace Service.ContractTests.Mocks
{
    public class IntegrationApiRestClient : RestClient
    {
        public IntegrationApiRestClient(IHttpSender httpSender) : base(httpSender)
        {
        }

        public async Task PublishEvent(string entityName, string eventName, int major, int minor, dynamic eventContent)
        {
            var relativeUrl = $"BusinessEvents/Publish/{entityName}/{eventName}/{major}/{minor}";
            await PostNoResponseContentAsync(relativeUrl, eventContent);
        }
    }
}
