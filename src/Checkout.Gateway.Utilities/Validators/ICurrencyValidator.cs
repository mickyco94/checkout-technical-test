namespace Checkout.Gateway.Utilities.Validators
{
    public interface ICurrencyValidator
    {
        bool IsSupported(string currencyCode);
    }
}