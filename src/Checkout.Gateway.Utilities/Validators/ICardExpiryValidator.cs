namespace Checkout.Gateway.Utilities.Validators
{
    public interface ICardExpiryValidator
    {
        bool IsValid(string input);
        bool IsExpired(string input);
    }
}