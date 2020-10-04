using FluentValidation;

namespace Checkout.Gateway.Service.Commands.CreatePayment
{
    public class CreatePaymentValidator : AbstractValidator<CreatePaymentRequest>
    {
        public CreatePaymentValidator()
        {
            CascadeMode = CascadeMode.Stop;
        }
    }
}
