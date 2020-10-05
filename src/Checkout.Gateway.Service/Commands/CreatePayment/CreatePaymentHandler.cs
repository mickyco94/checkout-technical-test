using Checkout.Gateway.Data.Models;
using Checkout.Gateway.Service.Commands.ProcessRejectedPayment;
using Checkout.Gateway.Service.Commands.ProcessSuccessfulPayment;
using Checkout.Gateway.Utilities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MockBank.API.Client;
using MockBank.API.Client.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Checkout.Gateway.Service.Commands.CreatePayment
{
    public class CreatePaymentHandler : IRequestHandler<CreatePaymentRequest, ApiResponse<CreatePaymentResponse>>
    {
        private readonly ILogger<CreatePaymentHandler> _logger;
        private readonly IMockBankApiClient _mockBankApiClient;
        private readonly IMediator _mediator;
        private readonly IMerchantContext _merchantContext;

        public CreatePaymentHandler(
            ILogger<CreatePaymentHandler> logger,
            IMockBankApiClient mockBankApiClient,
            IMediator mediator,
            IMerchantContext merchantContext)
        {
            _logger = logger;
            _mockBankApiClient = mockBankApiClient;
            _mediator = mediator;
            _merchantContext = merchantContext;
        }

        public async Task<ApiResponse<CreatePaymentResponse>> Handle(
            CreatePaymentRequest request,
            CancellationToken cancellationToken = default)
        {
            var mockBankResponse = await TransferFundsRequestToBank(request);

            switch (mockBankResponse.StatusCode)
            {
                case StatusCodes.Status200OK:
                    return await HandlePaymentSuccessful(request, mockBankResponse.SuccessResponse);
                case StatusCodes.Status422UnprocessableEntity:
                    return await HandlePaymentRejected(request, mockBankResponse.ErrorResponse);
                default:
                    return HandleUnknownBankResponse(mockBankResponse);
            }
        }

        private async Task<TransferFundsResponse> TransferFundsRequestToBank(CreatePaymentRequest request)
        {
            return await _mockBankApiClient.TransferFunds(new TransferFundsRequest
            {
                Currency = request.Currency,
                Amount = request.Amount,
                Source = new TransferFundsRequest.PaymentSource
                {
                    Cvv = request.Source.Cvv,
                    CardExpiry = request.Source.CardExpiry,
                    CardNumber = request.Source.CardNumber,
                },
                Recipient = new TransferFundsRequest.PaymentRecipient
                {
                    SortCode = request.Recipient.SortCode,
                    AccountNumber = request.Recipient.AccountNumber
                }
            });
        }

        //MCO: Here we are assuming that the bank has it's own idempotency methods that will allow us to safely retry in the case of a timeout, internal server error etc.
        private ApiResponse<CreatePaymentResponse> HandleUnknownBankResponse(TransferFundsResponse mockBankResponse)
        {
            _logger.LogCritical("Unknown response received from payment Response: {@mockBankResponse}", mockBankResponse);

            return ApiResponse<CreatePaymentResponse>.Fail(StatusCodes.Status502BadGateway, "ERR_DEP_FAIL", "A dependency has failed, your payment has not been processed. Please try again");
        }

        private async Task<ApiResponse<CreatePaymentResponse>> HandlePaymentRejected(CreatePaymentRequest request, TransferFundsErrorResponse bankErrorResponse)
        {
            var res = await _mediator.Send(new ProcessRejectedPaymentRequest
            {
                Source = new ProcessRejectedPaymentRequest.PaymentSource
                {
                    Cvv = request.Source.Cvv,
                    CardExpiry = request.Source.CardExpiry,
                    CardNumber = request.Source.CardNumber,
                },
                Recipient = new ProcessRejectedPaymentRequest.PaymentRecipient
                {
                    SortCode = request.Recipient.SortCode,
                    AccountNumber = request.Recipient.AccountNumber
                },
                Currency = request.Currency,
                Amount = request.Amount,
                Merchant = new ProcessRejectedPaymentRequest.MerchantDetails
                {
                    Id = _merchantContext.GetMerchantId(),
                },
                BankResponse = new ProcessRejectedPaymentRequest.BankPaymentResponse
                {
                    FailureReason = bankErrorResponse.Code
                }
            });

            return ApiResponse<CreatePaymentResponse>.Success(StatusCodes.Status201Created, new CreatePaymentResponse
            {
                Status = PaymentStatus.Rejected,
                PaymentId = res.Id
            });
        }

        private async Task<ApiResponse<CreatePaymentResponse>> HandlePaymentSuccessful(CreatePaymentRequest request,
            TransferFundsSuccessfulResponse bankResponse)
        {
            var res = await _mediator.Send(new ProcessSuccessfulPaymentRequest
            {
                Source = new ProcessSuccessfulPaymentRequest.PaymentSource
                {
                    Cvv = request.Source.Cvv,
                    CardExpiry = request.Source.CardExpiry,
                    CardNumber = request.Source.CardNumber,
                },
                Recipient = new ProcessSuccessfulPaymentRequest.PaymentRecipient
                {
                    SortCode = request.Recipient.SortCode,
                    AccountNumber = request.Recipient.AccountNumber
                },
                Currency = request.Currency,
                Amount = request.Amount,
                Merchant = new ProcessSuccessfulPaymentRequest.MerchantDetails
                {
                    Id = _merchantContext.GetMerchantId(),
                },
                BankResponse = new ProcessSuccessfulPaymentRequest.BankPaymentResponse
                {
                    TransactionId = bankResponse.Id.ToString()
                }
            });

            return ApiResponse<CreatePaymentResponse>.Success(StatusCodes.Status201Created, new CreatePaymentResponse
            {
                Status = PaymentStatus.Succeeded,
                PaymentId = res.Id
            });
        }
    }
}
