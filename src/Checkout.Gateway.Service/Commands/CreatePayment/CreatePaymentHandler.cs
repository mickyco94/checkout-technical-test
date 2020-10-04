using Checkout.Gateway.Utilities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Checkout.Gateway.Service.Commands.CreatePayment
{
    public class CreatePaymentHandler : IRequestHandler<CreatePaymentRequest, ApiResponse<CreatePaymentResponse>>
    {
        private readonly IGuid _guid;

        public CreatePaymentHandler(
            IGuid guid)
        {
            _guid = guid;
        }

        public async Task<ApiResponse<CreatePaymentResponse>> Handle(CreatePaymentRequest request,
            CancellationToken cancellationToken)
        {
            //Validate request
            // - Valid format for CVV, Expiry, Card Number

            //Send payment request to Bank
            //Trap any errors from bank API
            //Return payment_failed

            //Add to local db

            //Encrypt sensitive data

            //Log process

            //We wouldn't want to lose track of the payment attempt so rolling back isn't an option when bank fails.

            var paymentId = _guid.NewGuid().ToString();
            return ApiResponse<CreatePaymentResponse>.Success(200, new CreatePaymentResponse
            {
                PaymentId = paymentId
            });
        }
    }
}
