using Microsoft.Extensions.Options;
using System.Linq;

namespace Checkout.Gateway.Utilities.Validators
{
    public class CurrencyValidator : ICurrencyValidator
    {
        private readonly IOptionsMonitor<SupportedCurrencyProvider> _options;

        public CurrencyValidator(IOptionsMonitor<SupportedCurrencyProvider> options)
        {
            _options = options;
        }

        public bool IsSupported(string currencyCode) => _options.CurrentValue.SupportedCurrencies.Contains(currencyCode);
    }
}