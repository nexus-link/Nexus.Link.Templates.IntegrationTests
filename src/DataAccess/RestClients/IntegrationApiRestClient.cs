using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.BusinessEvents.Sdk.RestClients.Models;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Libraries.Web.RestClientHelper;

#pragma warning disable 1591

namespace DataAccess.RestClients
{
    public class IntegrationApiRestClient : RestClient
    {
        public IntegrationApiRestClient(IHttpSender httpSender) : base(httpSender)
        {
            // TODO: Auth
        }

        public async Task PublishEvent(string entityName, string eventName, int major, int minor, string client, JObject eventContent)
        {
            var relativeUrl = $"api/v1/BusinessEvents/Publish/{entityName}/{eventName}/{major}/{minor}?clientName={client}";
            await PostNoResponseContentAsync(relativeUrl, eventContent);
        }

        public async Task<PublicationTestResult> TestBenchPublish(string entityName, string eventName, int major, int minor, string client, JObject eventContent)
        {
            var relativeUrl = $"api/v1/TestBench/{entityName}/{eventName}/{major}/{minor}/Publish?clientName={client}";
            var result = await PostAsync<PublicationTestResult, dynamic>(relativeUrl, eventContent);
            return result;
        }

        public async Task<AuthenticationToken> CreateToken(string clientId, string clientSecret)
        {
            var relativeUrl = "api/v1/Authentication/Tokens";
            var authenticationCredentials = new AuthenticationCredentials { ClientId = clientId, ClientSecret = clientSecret };
            var result = await PostAsync<AuthenticationToken, AuthenticationCredentials>(relativeUrl, authenticationCredentials);
            return result;
        }
    }
}
