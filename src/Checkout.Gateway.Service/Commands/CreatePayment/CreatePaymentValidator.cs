using Checkout.Gateway.Utilities.Regex;
using Checkout.Gateway.Utilities.Validators;
using FluentValidation;

namespace Checkout.Gateway.Service.Commands.CreatePayment
{
    public class CreatePaymentValidator : AbstractValidator<CreatePaymentRequest>
    {
        public CreatePaymentValidator(
            ICurrencyValidator currencyValidator,
            ICvvRegex cvvRegex,
            ISortCodeRegex sortCodeRegex,
            IAccountNumberRegex accountNumberRegex,
            ICardExpiryValidator cardExpiryValidator,
            ICardNumberValidator cardNumberValidator)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Source)
                .NotNull()
                .WithErrorCode("ERR_SOURCE")
                .WithMessage("No payment source provided");

            RuleFor(x => x.Recipient)
                .NotNull()
                .WithErrorCode("ERR_RECIPIENT")
                .WithMessage("No payment recipient provided");

            RuleFor(x => x.Source.Cvv)
                .Must(cvvRegex.IsMatch)
                .WithErrorCode("ERR_CVV")
                .WithMessage("Invalid CVV");

            RuleFor(x => x.Currency)
                .Must(currencyValidator.IsSupported)
                .WithErrorCode("ERR_CURRENCY")
                .WithMessage("Specified currency is not supported");

            RuleFor(x => x.Source.CardNumber)
                .Must(cardNumberValidator.IsValid)
                .WithErrorCode("ERR_CARD_NO")
                .WithMessage("Invalid card number");

            RuleFor(x => x.Source.CardExpiry)
                .Must(cardExpiryValidator.IsValid)
                .WithErrorCode("ERR_CARD_EXP_FORMAT")
                .WithMessage("Invalid source card expiry");

            RuleFor(x => x.Source.CardExpiry)
                .Must(x => !cardExpiryValidator.IsExpired(x))
                .WithErrorCode("ERR_CARD_EXP_EXP")
                .WithMessage("Source card has expired");

            RuleFor(x => x.Recipient.AccountNumber)
                .Must(accountNumberRegex.IsMatch)
                .WithErrorCode("ERR_ACC_NO")
                .WithMessage("Invalid recipient account number");

            RuleFor(x => x.Recipient.AccountNumber)
                .Must(accountNumberRegex.IsMatch)
                .WithErrorCode("ERR_ACC_NO")
                .WithMessage("Invalid recipient account number");

            RuleFor(x => x.Recipient.SortCode)
                .Must(sortCodeRegex.IsMatch)
                .WithErrorCode("ERR_ACC_SORT_CODE")
                .WithMessage("Invalid recipient sort code");

        }
    }
}
