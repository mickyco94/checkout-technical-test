using Checkout.Gateway.Data.Abstractions;
using Checkout.Gateway.Data.Models;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Checkout.Gateway.Service.Commands.ProcessRejectedPayment
{
    public class ProcessRejectedPaymentHandler : INotificationHandler<PaymentRejectedEvent>
    {
        private readonly IPaymentRecordUpdater _paymentRecordUpdater;
        private readonly IPaymentRecordReader _paymentRecordReader;

        public ProcessRejectedPaymentHandler(
            IPaymentRecordUpdater paymentRecordUpdater,
            IPaymentRecordReader paymentRecordReader)
        {
            _paymentRecordUpdater = paymentRecordUpdater;
            _paymentRecordReader = paymentRecordReader;
        }

        public Task Handle(PaymentRejectedEvent paymentRejectedEvent, CancellationToken cancellationToken = default)
        {
            var paymentRecord = _paymentRecordReader.PaymentRecords.First(x => x.Id == paymentRejectedEvent.Id);

            paymentRecord.Status = PaymentStatus.Succeeded;
            paymentRecord.FailureReason = paymentRejectedEvent.BankResponse.FailureReason;

            _paymentRecordUpdater.Update(paymentRecord);

            return Task.CompletedTask;
        }
    }
}
