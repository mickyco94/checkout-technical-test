using Checkout.Gateway.Data.Abstractions;
using Checkout.Gateway.Utilities;
using Checkout.Gateway.Utilities.Encryption;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Checkout.Gateway.Service.Queries
{
    public class GetPaymentByIdHandler : IRequestHandler<GetPaymentByIdRequest, ApiResponse<GetPaymentByIdResponse>>
    {
        private readonly IMerchantEncryptionKeyGetter _encryptionKeys;
        private readonly IDecrypter _decrypter;
        private readonly IPaymentRecordReader _paymentRecordReader;
        private readonly IMerchantContext _merchantContext;

        public GetPaymentByIdHandler(
            IMerchantEncryptionKeyGetter encryptionKeys,
            IDecrypter decrypter,
            IPaymentRecordReader paymentRecordReader,
            IMerchantContext merchantContext)
        {
            _encryptionKeys = encryptionKeys;
            _decrypter = decrypter;
            _paymentRecordReader = paymentRecordReader;
            _merchantContext = merchantContext;
        }
        public Task<ApiResponse<GetPaymentByIdResponse>> Handle(GetPaymentByIdRequest request, CancellationToken cancellationToken = default)
        {
            var merchantId = _merchantContext.GetMerchantId();

            var merchantKey = _encryptionKeys.Key(merchantId);

            var paymentRecord = _paymentRecordReader
                .PaymentRecords
                .Where(record => record.MerchantId == merchantId)
                .Where(record => record.Id == request.Id)
                .Select(b => new
                {
                    b.Id,
                    DecryptedCvv = _decrypter.DecryptUtf8(b.Source.CvvEncrypted, merchantKey),
                    DecryptedCardExpiry = _decrypter.DecryptUtf8(b.Source.CardExpiryEncrypted, merchantKey),
                    DecryptedCardNumber = _decrypter.DecryptUtf8(b.Source.CardNumberEncrypted, merchantKey),
                    DecryptedSortCode = _decrypter.DecryptUtf8(b.Recipient.SortCodeEncrypted, merchantKey),
                    DecryptedAccountNumber = _decrypter.DecryptUtf8(b.Recipient.AccountNumberEncrypted, merchantKey),
                    b.FailureReason,
                    b.Amount,
                    b.Status,
                    b.CreatedAt,
                    b.Currency
                })
                .Select(record => new GetPaymentByIdResponse
                {
                    Id = record.Id,
                    Source = new GetPaymentByIdResponse.PaymentSource
                    {
                        Cvv = record.DecryptedCvv.Mask(3, 0, 'X'),
                        CardExpiry = record.DecryptedCardExpiry,
                        CardNumber = record.DecryptedCardNumber.Mask(12, 0, 'X'),
                    },
                    Recipient = new GetPaymentByIdResponse.PaymentRecipient
                    {
                        SortCode = record.DecryptedSortCode.Mask(4, 0, 'X'),
                        AccountNumber = record.DecryptedAccountNumber.Mask(6, 0, 'X')
                    },
                    Details = string.IsNullOrEmpty(record.FailureReason) ? null : new GetPaymentByIdResponse.BankResponse
                    {
                        FailureReason = record.FailureReason
                    },
                    Amount = record.Amount,
                    Status = record.Status,
                    CreatedAt = record.CreatedAt,
                    Currency = record.Currency,
                }).FirstOrDefault();

            if (paymentRecord == null)
            {
                return Task.FromResult(ApiResponse<GetPaymentByIdResponse>.Fail(StatusCodes.Status404NotFound,
                    "NOT_FOUND",
                    "Payment record not found"));
            }

            return Task.FromResult(ApiResponse<GetPaymentByIdResponse>.Success(StatusCodes.Status200OK, paymentRecord));
        }
    }
}
