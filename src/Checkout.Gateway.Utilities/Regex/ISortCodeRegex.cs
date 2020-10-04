namespace Checkout.Gateway.Utilities.Regex
{
    public interface ISortCodeRegex
    {
        bool IsMatch(string input);
    }
}