using Checkout.Gateway.Data.Abstractions;
using Checkout.Gateway.Data.Models;
using Checkout.Gateway.Utilities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Checkout.Gateway.Service.Commands.ProcessSuccessfulPayment
{
    public class ProcessSuccessfulPaymentHandler : IRequestHandler<ProcessSuccessfulPaymentRequest, ProcessSuccessfulPaymentResponse>
    {
        private readonly IPaymentRecordCreator _paymentRecordCreator;
        private readonly IGuid _guid;
        private readonly IDateTime _dateTime;

        public ProcessSuccessfulPaymentHandler(
            IPaymentRecordCreator paymentRecordCreator,
            IGuid guid,
            IDateTime dateTime)
        {
            _paymentRecordCreator = paymentRecordCreator;
            _guid = guid;
            _dateTime = dateTime;
        }

        public Task<ProcessSuccessfulPaymentResponse> Handle(ProcessSuccessfulPaymentRequest request, CancellationToken cancellationToken = default)
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
                Status = PaymentStatus.Succeeded,
                Amount = request.Amount,
                CreatedAt = _dateTime.UtcNow(),
                BankPaymentId = request.BankResponse.TransactionId,
                MerchantId = request.Merchant.Id
            };

            _paymentRecordCreator.Add(paymentRecord);

            return Task.FromResult(new ProcessSuccessfulPaymentResponse
            {
                Id = paymentRecord.Id
            });
        }
    }
}
