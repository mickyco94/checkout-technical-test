namespace Checkout.Gateway.Utilities.Encryption
{
    public interface IDecrypter
    {
        string DecryptUtf8(string data, byte[] key);
    }
}