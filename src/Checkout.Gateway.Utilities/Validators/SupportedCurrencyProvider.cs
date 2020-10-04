using System.Collections.Generic;

namespace Checkout.Gateway.Utilities.Validators
{
    public class SupportedCurrencyProvider
    {
        public IEnumerable<string> SupportedCurrencies { get; set; } = new List<string> { "gbp" };
    }
}