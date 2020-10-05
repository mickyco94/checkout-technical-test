using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;

namespace Checkout.Gateway.API.Authentication
{
    public class SimpleAuthenticationHandlerOptions : AuthenticationSchemeOptions
    {
        public Dictionary<string, string> MerchantKeys { get; set; }
    }
}