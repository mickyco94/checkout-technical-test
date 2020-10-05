using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Checkout.Gateway.API.Authentication
{
    //Not going to dwell too much on Auth since that's a whole separate system that is very complicated when correctly implemented
    //The implementation of Auth varies quite a bit depending on the needs of the domain, needless to say this implementation isn't production ready.
    //Simply a placeholder that allows us to demonstrate how auth can be used later within the application to restrict a clients access to resources.
    public class SimpleAuthenticationHandler : AuthenticationHandler<SimpleAuthenticationHandlerOptions>
    {
        public SimpleAuthenticationHandler(IOptionsMonitor<SimpleAuthenticationHandlerOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authHeader = Context.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader)) return Task.FromResult(AuthenticateResult.NoResult());

            if (Options.MerchantKeys.TryGetValue(authHeader, out var merchantId))
            {
                var claims = new List<Claim> { new Claim(ClaimTypes.Sid, merchantId) };

                var claimsIdentity = new ClaimsIdentity(claims, nameof(SimpleAuthenticationHandler));

                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                var ticket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            return Task.FromResult(AuthenticateResult.Fail("Invalid token"));
        }
    }
}
