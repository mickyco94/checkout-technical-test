namespace Checkout.Gateway.Service
{
    public interface IMerchantEncryptionKeyGetter
    {
        byte[] Key(string merchantId);
    }
}
