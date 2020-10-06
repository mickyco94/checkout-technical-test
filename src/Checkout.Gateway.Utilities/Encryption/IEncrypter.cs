namespace Checkout.Gateway.Utilities.Encryption
{
    public interface IEncrypter
    {
        string EncryptUtf8(string data, byte[] key);
    }
}