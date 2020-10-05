using Checkout.Gateway.Data.Models;

namespace Checkout.Gateway.Data.Abstractions
{
    public interface IPaymentRecordUpdater
    {
        void Update(PaymentRecord paymentRecord);
    }
}