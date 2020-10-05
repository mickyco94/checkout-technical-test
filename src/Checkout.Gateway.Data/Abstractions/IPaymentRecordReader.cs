using Checkout.Gateway.Data.Models;
using System.Linq;

namespace Checkout.Gateway.Data.Abstractions
{
    public interface IPaymentRecordReader
    {
        IQueryable<PaymentRecord> PaymentRecords { get; }
    }
}