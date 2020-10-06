using Checkout.Gateway.Utilities.Encryption;
using Microsoft.Extensions.Options;

namespace Checkout.Gateway.Service
{
    class MerchantEncryptionKeyGetter : IMerchantEncryptionKeyGetter
    {
        private readonly IOptionsMonitor<MerchantEncryptionKeys> _merchantEncryptionKeys;

        public MerchantEncryptionKeyGetter(IOptionsMonitor<MerchantEncryptionKeys> merchantEncryptionKeys)
        {
            _merchantEncryptionKeys = merchantEncryptionKeys;
        }

        public byte[] Key(string merchantId) => _merchantEncryptionKeys.CurrentValue.Values[merchantId];
    }
}