using Microsoft.Extensions.Caching.Memory;
using System;

namespace Checkout.Gateway.Utilities.Cache
{
    public class CacheWrapper : ICache
    {
        private readonly IMemoryCache _memoryCache;

        public CacheWrapper(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void Set(string key, string value, DateTimeOffset? absoluteExpiration = null)
        {
            if (absoluteExpiration.HasValue)
            {
                _memoryCache.Set(key, value, absoluteExpiration.Value);
            }
            else
            {
                _memoryCache.Set(key, value);
            }
        }

        public string Get(string key)
        {
            if (_memoryCache.TryGetValue<string>(key, out var value))
            {
                return value;
            }

            return null;
        }

        public void Delete(string key)
        {
            _memoryCache.Remove(key);
        }

        public bool Contains(string key)
        {
            return _memoryCache.TryGetValue(key, out _);
        }
    }
}