namespace MockBank.API.Models
{
    public class TransferFundsBankRequest
    {
        public PaymentSource Source { get; set; }
        public PaymentRecipient Recipient { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }

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
    }
}