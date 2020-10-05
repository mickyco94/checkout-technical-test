using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace Checkout.Gateway.Service
{
    public class MerchantContext : IMerchantContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MerchantContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetMerchantId() => _httpContextAccessor
                                             .HttpContext
                                             .User
                                             .FindFirst(a => a.Type == ClaimTypes.Sid)?.Value ?? throw new ArgumentNullException(nameof(ClaimTypes.Sid));
    }
}
