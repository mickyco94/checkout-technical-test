namespace Checkout.Gateway.Utilities.Validators
{
    public interface ICardNumberValidator
    {
        bool IsValid(string input);
    }
}