using Checkout.Gateway.Data.Abstractions;
using Checkout.Gateway.Data.Models;
using Checkout.Gateway.Utilities;
using Checkout.Gateway.Utilities.Encryption;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Checkout.Gateway.Service.Commands.ProcessRejectedPayment
{
    public class ProcessRejectedPaymentHandler : IRequestHandler<ProcessRejectedPaymentRequest, ProcessRejectedPaymentResponse>
    {
        private readonly IMerchantEncryptionKeyGetter _merchantEncryptionKeyGetter;
        private readonly IEncrypter _encrypter;
        private readonly IPaymentRecordCreator _paymentRecordCreator;
        private readonly IGuid _guid;
        private readonly IDateTime _dateTime;

        public ProcessRejectedPaymentHandler(
            IEncrypter encrypter,
            IPaymentRecordCreator paymentRecordCreator,
            IGuid guid,
            IDateTime dateTime,
            IMerchantEncryptionKeyGetter merchantEncryptionKeyGetter)
        {
            _encrypter = encrypter;
            _paymentRecordCreator = paymentRecordCreator;
            _guid = guid;
            _dateTime = dateTime;
            _merchantEncryptionKeyGetter = merchantEncryptionKeyGetter;
        }

        public Task<ProcessRejectedPaymentResponse> Handle(ProcessRejectedPaymentRequest request, CancellationToken cancellationToken = default)
        {
            var merchantKey = _merchantEncryptionKeyGetter.Key(request.Merchant.Id);

            var paymentRecord = new PaymentRecord
            {
                Id = _guid.NewGuid().ToString(),
                Source = new PaymentRecord.PaymentSource
                {
                    CardExpiryEncrypted = _encrypter.EncryptUtf8(request.Source.CardExpiry, merchantKey),
                    CardNumberEncrypted = _encrypter.EncryptUtf8(request.Source.CardNumber, merchantKey),
                    CvvEncrypted = _encrypter.EncryptUtf8(request.Source.Cvv, merchantKey)
                },
                Recipient = new PaymentRecord.PaymentRecipient
                {
                    AccountNumberEncrypted = _encrypter.EncryptUtf8(request.Recipient.AccountNumber, merchantKey),
                    SortCodeEncrypted = _encrypter.EncryptUtf8(request.Recipient.SortCode, merchantKey),
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
