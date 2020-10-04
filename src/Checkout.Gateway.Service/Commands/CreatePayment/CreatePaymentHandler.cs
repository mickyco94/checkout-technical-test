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
        private readonly IGuid _guid;

        public CreatePaymentHandler(
            ILogger<CreatePaymentHandler> logger,
            IMockBankApiClient mockBankApiClient,
            IGuid guid)
        {
            _logger = logger;
            _mockBankApiClient = mockBankApiClient;
            _guid = guid;
        }

        public async Task<ApiResponse<CreatePaymentResponse>> Handle(
            CreatePaymentRequest request,
            CancellationToken cancellationToken = default)
        {
            var mockBankResponse = await TransferFundsRequestToBank(request);

            switch (mockBankResponse.StatusCode)
            {
                case StatusCodes.Status200OK:
                    return HandlePaymentSuccessful(mockBankResponse, request);
                case StatusCodes.Status422UnprocessableEntity:
                    return HandlePaymentRejected(mockBankResponse, request);
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

        private ApiResponse<CreatePaymentResponse> HandlePaymentRejected(TransferFundsResponse mockBankResponse,
            CreatePaymentRequest request)
        {
            return ApiResponse<CreatePaymentResponse>.Success(StatusCodes.Status201Created, new CreatePaymentResponse
            {
                Status = PaymentStatus.Rejected,
                PaymentId = _guid.NewGuid().ToString()
            });
        }

        private ApiResponse<CreatePaymentResponse> HandlePaymentSuccessful(TransferFundsResponse mockBankResponse,
            CreatePaymentRequest request)
        {
            return ApiResponse<CreatePaymentResponse>.Success(StatusCodes.Status201Created, new CreatePaymentResponse
            {
                Status = PaymentStatus.Succeeded,
                PaymentId = _guid.NewGuid().ToString()
            });
        }
    }
}
