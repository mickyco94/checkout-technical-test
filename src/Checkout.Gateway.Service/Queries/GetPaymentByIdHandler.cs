using Checkout.Gateway.Data.Abstractions;
using Checkout.Gateway.Utilities;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Checkout.Gateway.Service.Queries
{
    public class GetPaymentByIdHandler : IRequestHandler<GetPaymentByIdRequest, ApiResponse<GetPaymentByIdResponse>>
    {
        private readonly IPaymentRecordReader _paymentRecordReader;
        private readonly IMerchantContext _merchantContext;

        public GetPaymentByIdHandler(
            IPaymentRecordReader paymentRecordReader,
            IMerchantContext merchantContext)
        {
            _paymentRecordReader = paymentRecordReader;
            _merchantContext = merchantContext;
        }
        public Task<ApiResponse<GetPaymentByIdResponse>> Handle(GetPaymentByIdRequest request, CancellationToken cancellationToken = default)
        {
            var paymentRecord = _paymentRecordReader
                .PaymentRecords
                .Where(record => record.MerchantId == _merchantContext.GetMerchantId())
                .Where(record => record.Id == request.Id)
                .Select(record => new GetPaymentByIdResponse
                {
                    Id = record.Id,
                    Source = new GetPaymentByIdResponse.PaymentSource
                    {
                        Cvv = record.Source.CvvEncrypted.Mask(3, 0, 'X'),
                        CardExpiry = record.Source.CardExpiryEncrypted,
                        CardNumber = record.Source.CardNumberEncrypted.Mask(12, 0, 'X'),
                    },
                    Recipient = new GetPaymentByIdResponse.PaymentRecipient
                    {
                        SortCode = record.Recipient.SortCodeEncrypted.Mask(4, 0, 'X'),
                        AccountNumber = record.Recipient.AccountNumberEncrypted.Mask(6, 0, 'X')
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
