using Checkout.Gateway.Data.Abstractions;
using Checkout.Gateway.Data.Models;
using Checkout.Gateway.Utilities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Checkout.Gateway.Service.Commands.ProcessRejectedPayment
{
    public class ProcessRejectedPaymentHandler : IRequestHandler<ProcessRejectedPaymentRequest, ProcessRejectedPaymentResponse>
    {
        private readonly IPaymentRecordCreator _paymentRecordCreator;
        private readonly IGuid _guid;
        private readonly IDateTime _dateTime;

        public ProcessRejectedPaymentHandler(
            IPaymentRecordCreator paymentRecordCreator,
            IGuid guid,
            IDateTime dateTime)
        {
            _paymentRecordCreator = paymentRecordCreator;
            _guid = guid;
            _dateTime = dateTime;
        }

        public Task<ProcessRejectedPaymentResponse> Handle(ProcessRejectedPaymentRequest request, CancellationToken cancellationToken = default)
        {
            var paymentRecord = new PaymentRecord
            {
                Id = _guid.NewGuid().ToString(),
                Source = new PaymentRecord.PaymentSource
                {
                    CardExpiryEncrypted = request.Source.CardExpiry,
                    CardNumberEncrypted = request.Source.CardNumber,
                    CvvEncrypted = request.Source.Cvv
                },
                Recipient = new PaymentRecord.PaymentRecipient
                {
                    AccountNumberEncrypted = request.Recipient.AccountNumber,
                    SortCodeEncrypted = request.Recipient.SortCode,
                },
                Currency = request.Currency,
                Status = PaymentStatus.Rejected,
                Amount = request.Amount,
                CreatedAt = _dateTime.UtcNow(),
                MerchantId = request.Merchant.Id,
                FailureReason = request.BankResponse.FailureReason,
            };

            _paymentRecordCreator.Add(paymentRecord);

            return Task.FromResult(new ProcessRejectedPaymentResponse
            {
                Id = paymentRecord.Id
            });
        }
    }
}
