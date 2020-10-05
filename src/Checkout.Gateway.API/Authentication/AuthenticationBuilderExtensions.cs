using Microsoft.AspNetCore.Authentication;
using System;

namespace Checkout.Gateway.API.Authentication
{
    public static class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddApiKeyAuthentication(this AuthenticationBuilder builder, Action<SimpleAuthenticationHandlerOptions> options, string schemeName = AuthenticationConstants.ApiKeyAuthenticationScheme)
        {
            return builder.AddScheme<SimpleAuthenticationHandlerOptions, SimpleAuthenticationHandler>(schemeName, options);
        }

    }
}