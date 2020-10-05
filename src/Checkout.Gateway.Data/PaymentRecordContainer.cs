using Checkout.Gateway.Data.Abstractions;
using Checkout.Gateway.Data.Models;
using System;
using System.Linq;

namespace Checkout.Gateway.Data
{
    public class PaymentRecordContainer : IPaymentRecordReader, IPaymentRecordUpdater, IPaymentRecordCreator
    {
        private readonly MockDocumentDb _mockDocumentDb;

        public PaymentRecordContainer(MockDocumentDb mockDocumentDb)
        {
            _mockDocumentDb = mockDocumentDb;
        }

        public void Add(PaymentRecord record)
        {
            if (_mockDocumentDb.Payments.Any(a => a.Id == record.Id))
            {
                throw new ArgumentException(nameof(record));
            }

            _mockDocumentDb.Payments.Add(record);
        }

        public IQueryable<PaymentRecord> PaymentRecords => _mockDocumentDb.Payments.AsQueryable();
        public void Update(PaymentRecord paymentRecord)
        {
            var existing = _mockDocumentDb.Payments.FirstOrDefault(x => x.Id == paymentRecord.Id);
            _mockDocumentDb.Payments.Remove(existing);
            _mockDocumentDb.Payments.Add(paymentRecord);
        }
    }
}