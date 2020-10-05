using System;

namespace Checkout.Gateway.Utilities.Cache
{
    public interface ICache
    {
        void Set(string key, string value, DateTimeOffset? absoluteExpiration);
        string Get(string key);
        bool Contains(string key);
        void Delete(string key);
    }
}
