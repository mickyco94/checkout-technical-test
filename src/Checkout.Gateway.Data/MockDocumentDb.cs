using Checkout.Gateway.Data.Models;
using System.Collections.Generic;

namespace Checkout.Gateway.Data
{
    public class MockDocumentDb
    {
        public MockDocumentDb()
        {
            Payments = new List<PaymentRecord>();
        }

        public List<PaymentRecord> Payments { get; }
    }
}
