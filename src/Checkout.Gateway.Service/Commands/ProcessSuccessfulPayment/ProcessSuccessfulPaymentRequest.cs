using MediatR;

namespace Checkout.Gateway.Service.Commands.ProcessSuccessfulPayment
{
    public class ProcessSuccessfulPaymentRequest : IRequest<ProcessSuccessfulPaymentResponse>
    {
        public PaymentSource Source { get; set; }
        public PaymentRecipient Recipient { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public BankPaymentResponse BankResponse { get; set; }

        public MerchantDetails Merchant { get; set; }

        public class PaymentSource
        {
            public string CardNumber { get; set; }
            public string CardExpiry { get; set; }
            public string Cvv { get; set; }
        }

        public class PaymentRecipient
        {
            public string AccountNumber { get; set; }
            public string SortCode { get; set; }
        }


        public class BankPaymentResponse
        {
            public string TransactionId { get; set; }
        }

        public class MerchantDetails
        {
            public string Id { get; set; }
        }
    }
}