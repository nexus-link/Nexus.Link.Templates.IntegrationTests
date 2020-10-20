using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Libraries.Web.RestClientHelper;

#pragma warning disable 1591

namespace Service.RestClients
{
    public class IntegrationApiRestClient : RestClient
    {
        public IntegrationApiRestClient(IHttpSender httpSender) : base(httpSender)
        {
            // TODO: Auth
        }

        public async Task PublishEvent(string entityName, string eventName, int major, int minor, dynamic eventContent)
        {
            var relativeUrl = $"BusinessEvents/Publish/{entityName}/{eventName}/{major}/{minor}";
            await PostNoResponseContentAsync(relativeUrl, eventContent);
        }

        public async Task<AuthenticationToken> CreateToken(string clientId, string clientSecret)
        {
            var relativeUrl = "Authentication/Tokens";
            var authenticationCredentials = new AuthenticationCredentials { ClientId = clientId, ClientSecret = clientSecret };
            var result = await PostAsync<AuthenticationToken, AuthenticationCredentials>(relativeUrl, authenticationCredentials);
            return result;
        }
    }
}
