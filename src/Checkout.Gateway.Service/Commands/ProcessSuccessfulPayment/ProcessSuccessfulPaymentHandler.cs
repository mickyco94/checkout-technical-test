using Checkout.Gateway.Data.Abstractions;
using Checkout.Gateway.Data.Models;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Checkout.Gateway.Service.Commands.ProcessSuccessfulPayment
{
    public class ProcessSuccessfulPaymentHandler : INotificationHandler<PaymentSucceededEvent>
    {
        private readonly IPaymentRecordUpdater _paymentRecordUpdater;
        private readonly IPaymentRecordReader _paymentRecordReader;

        public ProcessSuccessfulPaymentHandler(
            IPaymentRecordReader paymentRecordReader,
            IPaymentRecordUpdater paymentRecordUpdater)
        {
            _paymentRecordReader = paymentRecordReader;
            _paymentRecordUpdater = paymentRecordUpdater;
        }

        public Task Handle(PaymentSucceededEvent paymentSucceededEvent, CancellationToken cancellationToken = default)
        {
            var paymentRecord = _paymentRecordReader.PaymentRecords.First(x => x.Id == paymentSucceededEvent.Id);

            paymentRecord.Status = PaymentStatus.Succeeded;
            paymentRecord.BankPaymentId = paymentSucceededEvent.BankResponse.TransactionId;

            _paymentRecordUpdater.Update(paymentRecord);

            return Task.CompletedTask;
        }
    }
}
