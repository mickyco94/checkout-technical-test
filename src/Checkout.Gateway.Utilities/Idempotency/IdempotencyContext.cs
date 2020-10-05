using Checkout.Gateway.Utilities.Cache;
using Microsoft.AspNetCore.Http;

namespace Checkout.Gateway.Utilities.Idempotency
{
    public class IdempotencyContext : IIdempotencyContext
    {
        private readonly IDateTime _dateTime;
        private readonly ICache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public const string IdempotencyHeaderKey = "Idempotency-Key";

        //TODO: Make configurable
        private const int IdempotencyKeyExpiryTimeInHours = 24;

        public IdempotencyContext(
            IDateTime dateTime,
            ICache cache,
            IHttpContextAccessor httpContextAccessor)
        {
            _dateTime = dateTime;
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
        }

        private string IdempotencyKey => _httpContextAccessor.HttpContext.Request.Headers[IdempotencyHeaderKey];

        public void InvalidateToken()
        {
            if (string.IsNullOrEmpty(IdempotencyKey)) return;

            _cache.Set(IdempotencyKey, _dateTime.UtcNow().ToString("o"), _dateTime.UtcNow().AddHours(IdempotencyKeyExpiryTimeInHours));
        }

        public void RollbackInvalidation()
        {
            if (string.IsNullOrEmpty(IdempotencyKey)) return;

            _cache.Delete(IdempotencyKey);
        }

        public bool RequestAlreadyProcessed()
        {
            return !string.IsNullOrEmpty(IdempotencyKey) && _cache.Contains(IdempotencyKey);
        }
    }
}