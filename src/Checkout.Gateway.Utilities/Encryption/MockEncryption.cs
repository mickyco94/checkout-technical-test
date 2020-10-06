using System.Text;

namespace Checkout.Gateway.Utilities.Encryption
{
    public class MockEncryption : IEncrypter, IDecrypter
    {
        public string EncryptUtf8(string data, byte[] key)
        {
            return data + "_KEY_" + Encoding.UTF8.GetString(key);
        }

        public string DecryptUtf8(string data, byte[] key)
        {
            var keyStringLength = Encoding.UTF8.GetCharCount(key);
            return data.Substring(0, data.Length - 5 - keyStringLength);
        }
    }
}