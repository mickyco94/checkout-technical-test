using MediatR;

namespace Checkout.Gateway.Service.Commands.ProcessRejectedPayment
{
    public class PaymentRejectedEvent : INotification
    {
        public string Id { get; set; }
        public BankTransactionResponse BankResponse { get; set; }
        public class BankTransactionResponse
        {
            public string FailureReason { get; set; }
        }
    }
}