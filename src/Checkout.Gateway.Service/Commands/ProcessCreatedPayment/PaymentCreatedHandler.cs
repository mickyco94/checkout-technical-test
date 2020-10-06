using System.Threading;
using System.Threading.Tasks;
using Checkout.Gateway.Service.Commands.CreatePayment;
using Checkout.Gateway.Service.Commands.ProcessRejectedPayment;
using Checkout.Gateway.Service.Commands.ProcessSuccessfulPayment;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MockBank.API.Client;
using MockBank.API.Client.Models;

namespace Checkout.Gateway.Service.Commands.ProcessCreatedPayment
{
    public class PaymentCreatedHandler : INotificationHandler<PaymentCreatedEvent>
    {
        private readonly ILogger<PaymentCreatedHandler> _logger;
        private readonly IMockBankApiClient _mockBankApiClient;
        private readonly IMediator _mediator;

        public PaymentCreatedHandler(
            ILogger<PaymentCreatedHandler> logger,
            IMockBankApiClient mockBankApiClient,
            IMediator mediator)
        {
            _logger = logger;
            _mockBankApiClient = mockBankApiClient;
            _mediator = mediator;
        }

        private async Task<TransferFundsResponse> TransferFundsRequestToBank(PaymentCreatedEvent request)
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
        private void HandleUnknownBankResponse(TransferFundsResponse mockBankResponse)
        {
            _logger.LogCritical("Unknown response received from payment Response: {@mockBankResponse}", mockBankResponse);
        }

        private async Task HandlePaymentRejected(string id, TransferFundsErrorResponse bankErrorResponse)
        {
            await _mediator.Publish(new PaymentRejectedEvent
            {
                Id = id,
                BankResponse = new PaymentRejectedEvent.BankTransactionResponse
                {
                    FailureReason = bankErrorResponse.Code
                }
            });
        }

        private async Task HandlePaymentSuccessful(string id, TransferFundsSuccessfulResponse bankResponse)
        {
            await _mediator.Publish(new PaymentSucceededEvent
            {
                Id = id,
                BankResponse = new PaymentSucceededEvent.BankPaymentResponse
                {
                    TransactionId = bankResponse.Id.ToString()
                },
            });
        }

        public async Task Handle(PaymentCreatedEvent notification, CancellationToken cancellationToken)
        {
            var mockBankResponse = await TransferFundsRequestToBank(notification);

            switch (mockBankResponse.StatusCode)
            {
                case StatusCodes.Status200OK:
                    await HandlePaymentSuccessful(notification.Id, mockBankResponse.SuccessResponse);
                    break;
                case StatusCodes.Status422UnprocessableEntity:
                    await HandlePaymentRejected(notification.Id, mockBankResponse.ErrorResponse);
                    break;
                default:
                    HandleUnknownBankResponse(mockBankResponse);
                    break;
            }
        }
    }
}
