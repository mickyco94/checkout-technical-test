using MediatR;

namespace Checkout.Gateway.Service.Commands.ProcessSuccessfulPayment
{
    public class PaymentSucceededEvent : INotification
    {
        public string Id { get; set; }
        public BankPaymentResponse BankResponse { get; set; }

        public class BankPaymentResponse
        {
            public string TransactionId { get; set; }
        }
    }
}