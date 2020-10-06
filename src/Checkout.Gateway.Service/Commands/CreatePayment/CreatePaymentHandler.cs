using Checkout.Gateway.Data.Abstractions;
using Checkout.Gateway.Data.Models;
using Checkout.Gateway.Service.Commands.ProcessCreatedPayment;
using Checkout.Gateway.Utilities;
using Checkout.Gateway.Utilities.Encryption;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Checkout.Gateway.Service.Commands.CreatePayment
{
    public class CreatePaymentHandler : IRequestHandler<CreatePaymentRequest, ApiResponse<CreatePaymentResponse>>
    {
        private readonly IPaymentRecordCreator _paymentRecordCreator;
        private readonly IGuid _guid;
        private readonly IEncrypter _encrypter;
        private readonly IMerchantEncryptionKeyGetter _merchantEncryptionKeyGetter;
        private readonly IMerchantContext _merchantContext;
        private readonly IMediator _mediator;
        private readonly IDateTime _dateTime;

        public CreatePaymentHandler(IPaymentRecordCreator paymentRecordCreator,
            IGuid guid,
            IEncrypter encrypter,
            IMerchantEncryptionKeyGetter merchantEncryptionKeyGetter,
            IMerchantContext merchantContext,
            IMediator mediator,
            IDateTime dateTime)
        {
            _paymentRecordCreator = paymentRecordCreator;
            _guid = guid;
            _encrypter = encrypter;
            _merchantEncryptionKeyGetter = merchantEncryptionKeyGetter;
            _merchantContext = merchantContext;
            _mediator = mediator;
            _dateTime = dateTime;
        }

        public async Task<ApiResponse<CreatePaymentResponse>> Handle(CreatePaymentRequest request, CancellationToken cancellationToken)
        {
            var merchantId = _merchantContext.GetMerchantId();

            var merchantKey = _merchantEncryptionKeyGetter.Key(merchantId);

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
                Status = PaymentStatus.Pending,
                Amount = request.Amount,
                CreatedAt = _dateTime.UtcNow(),
                MerchantId = merchantId,
            };

            _paymentRecordCreator.Add(paymentRecord);

            await _mediator.Publish(new PaymentCreatedEvent
            {
                Id = paymentRecord.Id,
                Source = new PaymentCreatedEvent.PaymentSource
                {
                    CardExpiry = request.Source.CardExpiry,
                    CardNumber = request.Source.CardNumber,
                    Cvv = request.Source.Cvv,
                },
                Recipient = new PaymentCreatedEvent.PaymentRecipient
                {
                    SortCode = request.Recipient.SortCode,
                    AccountNumber = request.Recipient.AccountNumber
                },
                Amount = request.Amount,
                Currency = request.Currency
            }, cancellationToken);

            return ApiResponse<CreatePaymentResponse>.Success(StatusCodes.Status202Accepted, new CreatePaymentResponse
            {
                PaymentId = paymentRecord.Id
            });
        }
    }
}
