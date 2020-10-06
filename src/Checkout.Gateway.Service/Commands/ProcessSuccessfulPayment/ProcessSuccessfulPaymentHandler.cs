using Checkout.Gateway.Data.Abstractions;
using Checkout.Gateway.Data.Models;
using Checkout.Gateway.Utilities;
using Checkout.Gateway.Utilities.Encryption;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Checkout.Gateway.Service.Commands.ProcessSuccessfulPayment
{
    public class ProcessSuccessfulPaymentHandler : IRequestHandler<ProcessSuccessfulPaymentRequest, ProcessSuccessfulPaymentResponse>
    {
        private readonly IMerchantEncryptionKeyGetter _merchantEncryptionKeyGetter;
        private readonly IEncrypter _encrypter;
        private readonly IPaymentRecordCreator _paymentRecordCreator;
        private readonly IGuid _guid;
        private readonly IDateTime _dateTime;

        public ProcessSuccessfulPaymentHandler(
            IMerchantEncryptionKeyGetter merchantEncryptionKeyGetter,
            IEncrypter encrypter,
            IPaymentRecordCreator paymentRecordCreator,
            IGuid guid,
            IDateTime dateTime)
        {
            _merchantEncryptionKeyGetter = merchantEncryptionKeyGetter;
            _encrypter = encrypter;
            _paymentRecordCreator = paymentRecordCreator;
            _guid = guid;
            _dateTime = dateTime;
        }

        public Task<ProcessSuccessfulPaymentResponse> Handle(ProcessSuccessfulPaymentRequest request, CancellationToken cancellationToken = default)
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
