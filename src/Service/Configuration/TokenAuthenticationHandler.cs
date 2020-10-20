using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
#pragma warning disable 1591

namespace Service.Configuration
{
    public class TokenAuthenticationHandler : AuthenticationHandler<TokenAuthenticationOptions>
    {
        public IServiceProvider ServiceProvider { get; set; }

        public TokenAuthenticationHandler(IOptionsMonitor<TokenAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IServiceProvider serviceProvider)
            : base(options, logger, encoder, clock)
        {
            ServiceProvider = serviceProvider;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authHeader = Request.Headers.Keys.Contains("Authorization") ? Request.Headers["Authorization"].First() : null;
            var encodedAuth = (authHeader != null && authHeader.StartsWith("Basic ")) ? authHeader.Substring("Basic ".Length).Trim() : null;
            if (string.IsNullOrWhiteSpace(encodedAuth))
            {
                return Task.FromResult(AuthenticateResult.Fail("Basic Authorization header missing"));
            }

            var configuration = (IConfiguration) ServiceProvider.GetService(typeof(IConfiguration));
            var (username, password) = DecodeUserIdAndPassword(encodedAuth);

            if (!(username == configuration["Service:Username"] && password == configuration["Service:Password"]))
            {
                return Task.FromResult(AuthenticateResult.Fail("Authentication failed, bad credentials"));
            }

            var claims = new[] { new Claim(ClaimTypes.Name, username, ClaimValueTypes.String, "Basic") };
            var identity = new ClaimsIdentity(claims, nameof(TokenAuthenticationHandler));
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), "Basic");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        private static (string userid, string password) DecodeUserIdAndPassword(string encodedAuth)
        {
            var userpass = Encoding.UTF8.GetString(Convert.FromBase64String(encodedAuth));
            var separator = userpass.IndexOf(':');
            if (separator == -1) return (null, null);
            return (userpass.Substring(0, separator), userpass.Substring(separator + 1));
        }
    }

    public class TokenAuthenticationOptions : AuthenticationSchemeOptions
    {
    }
}
