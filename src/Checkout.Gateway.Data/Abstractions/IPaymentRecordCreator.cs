using Checkout.Gateway.Data.Models;

namespace Checkout.Gateway.Data.Abstractions
{
    public interface IPaymentRecordCreator
    {
        void Add(PaymentRecord record);
    }
}