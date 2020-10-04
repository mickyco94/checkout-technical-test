namespace Checkout.Gateway.Utilities.Regex
{
    public interface IAccountNumberRegex
    {
        bool IsMatch(string input);
    }
}