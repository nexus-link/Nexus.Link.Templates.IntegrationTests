using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using DataAccess.RestClients;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Libraries.Web.Platform.Authentication;
using Nexus.Link.Libraries.Web.RestClientHelper;

#pragma warning disable 1591

namespace Service.Configuration
{
    public class TokenRefresher : ServiceClientCredentials, ITokenRefresherWithServiceClient
    {
        private readonly AuthenticationCredentials _clientCredentials;
        private readonly IntegrationApiRestClient _integrationApiClient;

        private static readonly ObjectCache Cache = MemoryCache.Default;

        public TokenRefresher(IConfiguration configuration, string clientId, string clientSecret)
        {
            _clientCredentials = new AuthenticationCredentials { ClientId = clientId, ClientSecret = clientSecret };
            var platformSettings = configuration.GetSection("Platform").Get<PlatformSettings>();
            _integrationApiClient = new IntegrationApiRestClient(new HttpSender(platformSettings.IntegrationApiUrl));
        }

        public override async Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            InternalContract.RequireNotNull(request, nameof(request));
            var token = await GetJwtTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            await base.ProcessHttpRequestAsync(request, cancellationToken);
        }

        public async Task<AuthenticationToken> GetJwtTokenAsync()
        {
            var key = $"{nameof(TokenRefresher)}.Token.{_clientCredentials.ClientId}";
            if (Cache[key] is AuthenticationToken token) return token;
            token = await _integrationApiClient.CreateToken(_clientCredentials.ClientId, _clientCredentials.ClientSecret);
            Cache.Add(key, token, DateTimeOffset.Now.AddHours(1));
            return token;
        }

        public ServiceClientCredentials GetServiceClient() => this;
    }
}