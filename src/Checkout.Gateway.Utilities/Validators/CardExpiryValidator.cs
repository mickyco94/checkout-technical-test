using System;
using System.Globalization;

namespace Checkout.Gateway.Utilities.Validators
{
    public class CardExpiryValidator : ICardExpiryValidator
    {
        private readonly IDateTime _dateTime;

        public CardExpiryValidator(IDateTime dateTime)
        {
            _dateTime = dateTime;
        }

        private const string CardExpiryFormat = "MM/yyyy";

        public bool IsValid(string input) => DateTime.TryParseExact(input,
            CardExpiryFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _);

        public bool IsExpired(string input)
        {
            if (!IsValid(input))
            {
                throw new ArgumentException(nameof(input));
            }

            return DateTime.TryParseExact(input,
                       CardExpiryFormat,
                       CultureInfo.InvariantCulture,
                       DateTimeStyles.None,
                       out var parsed) && _dateTime.UtcNow() > ActualCardExpiry(parsed);
        }

        private static DateTime ActualCardExpiry(DateTime parsed)
        {
            return parsed.EndOfMonth();
        }
    }
}