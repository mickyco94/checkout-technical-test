using System;

namespace Checkout.Gateway.Data.Models
{
    public class PaymentRecord
    {
        public string Id { get; set; }
        public PaymentSource Source { get; set; }
        public PaymentRecipient Recipient { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string MerchantId { get; set; }
        public class PaymentSource
        {
            public string CardNumberEncrypted { get; set; }
            public string CardExpiryEncrypted { get; set; }
            public string CvvEncrypted { get; set; }
        }
        public class PaymentRecipient
        {
            public string AccountNumberEncrypted { get; set; }
            public string SortCodeEncrypted { get; set; }
        }
        public string BankPaymentId { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string FailureReason { get; set; }
    }
}