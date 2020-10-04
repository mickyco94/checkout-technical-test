namespace Checkout.Gateway.Utilities.Regex
{
    public interface ICvvRegex
    {
        bool IsMatch(string input);
    }
}