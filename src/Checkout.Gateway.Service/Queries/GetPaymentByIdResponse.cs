using System;
using Checkout.Gateway.Data.Models;

namespace Checkout.Gateway.Service.Queries
{
    public class GetPaymentByIdResponse
    {
        public string Id { get; set; }
        public PaymentSource Source { get; set; }
        public PaymentRecipient Recipient { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }

        public BankResponse Details { get; set; }

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

        public class BankResponse
        {
            public string FailureReason { get; set; }
        }
        public PaymentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}