using AutoFixture;
using Checkout.Gateway.Data.Models;
using Checkout.Gateway.Service.Commands.CreatePayment;
using Checkout.Gateway.Service.Commands.ProcessRejectedPayment;
using Checkout.Gateway.Service.Commands.ProcessSuccessfulPayment;
using FluentAssertions;
using FluentAssertions.Primitives;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MockBank.API.Client;
using MockBank.API.Client.Models;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace Checkout.Gateway.Service.Tests.Commands
{
    [TestFixture]
    public class CreatePaymentHandlerTests
    {
        private MockRepository _mockRepository;
        private IFixture _fixture;

        private Mock<ILogger<CreatePaymentHandler>> _logger;
        private Mock<IMockBankApiClient> _mockBankApiClient;
        private Mock<IMediator> _mediator;
        private Mock<IMerchantContext> _merchantContext;

        private CreatePaymentHandler _createPaymentHandler;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _fixture = new Fixture();

            // Mock setup
            _logger = _mockRepository.Create<ILogger<CreatePaymentHandler>>(MockBehavior.Loose);
            _mockBankApiClient = _mockRepository.Create<IMockBankApiClient>();
            _mediator = _mockRepository.Create<IMediator>();
            _merchantContext = _mockRepository.Create<IMerchantContext>();

            // Mock default
            SetupMockDefaults();

            // Sut instantiation
            _createPaymentHandler = new CreatePaymentHandler(
                _logger.Object,
                _mockBankApiClient.Object,
                _mediator.Object,
                _merchantContext.Object
            );
        }
        private void SetupMockDefaults()
        {
            _mediator
                .Setup(x => x.Send(It.IsAny<ProcessSuccessfulPaymentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fixture.Create<ProcessSuccessfulPaymentResponse>());

            _mediator
                .Setup(x => x.Send(It.IsAny<ProcessRejectedPaymentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fixture.Create<ProcessRejectedPaymentResponse>());

            _merchantContext
                .Setup(x => x.GetMerchantId())
                .Returns(_fixture.Create<string>());

            _mockBankApiClient
                .Setup(x => x.TransferFunds(It.IsAny<TransferFundsRequest>()))
                .ReturnsAsync(TransferFundsResponse.Successful(_fixture.Create<TransferFundsSuccessfulResponse>()));
        }

        [Test]
        public async Task Handle_BankResponseSuccessful_ReturnsExpectedSuccessResponse()
        {
            //arrange
            var processSuccessfulPaymentResponse = _fixture.Create<ProcessSuccessfulPaymentResponse>();

            _mediator
                .Setup(x => x.Send(It.IsAny<ProcessSuccessfulPaymentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(processSuccessfulPaymentResponse);

            _mockBankApiClient
                .Setup(x => x.TransferFunds(It.IsAny<TransferFundsRequest>()))
                .ReturnsAsync(TransferFundsResponse.Successful(_fixture.Create<TransferFundsSuccessfulResponse>()));

            //act
            var res = await _createPaymentHandler.Handle(_fixture.Create<CreatePaymentRequest>());

            //assert
            res.StatusCode.Should().Be(StatusCodes.Status201Created);
            res.SuccessResponse.Status.Should().Be(PaymentStatus.Succeeded);
            res.SuccessResponse.PaymentId.Should().Be(processSuccessfulPaymentResponse.Id);
        }

        [Test]
        public async Task Handle_BankResponseSuccessful_SendsProcessSuccessfulPaymentRequest()
        {
            //arrange
            var merchantId = _fixture.Create<string>();

            _merchantContext.Setup(x => x.GetMerchantId()).Returns(merchantId);

            var successfulTransferResponse = _fixture.Create<TransferFundsSuccessfulResponse>();

            _mockBankApiClient
                .Setup(x => x.TransferFunds(It.IsAny<TransferFundsRequest>()))
                .ReturnsAsync(TransferFundsResponse.Successful(successfulTransferResponse));

            var request = _fixture.Create<CreatePaymentRequest>();

            var expected = new ProcessSuccessfulPaymentRequest
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
                    Id = merchantId,
                },
                BankResponse = new ProcessSuccessfulPaymentRequest.BankPaymentResponse
                {
                    TransactionId = successfulTransferResponse.Id.ToString()
                }
            };

            //act
            await _createPaymentHandler.Handle(request);

            //assert
            _mediator.Verify(x => x.Send(It.Is<ProcessSuccessfulPaymentRequest>(req => req.Should().BeEquivalentToBool(expected)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_BankResponseRejected_ReturnsRejectedStatus()
        {
            //arrange
            var processRejectedPaymentResponse = _fixture.Create<ProcessRejectedPaymentResponse>();

            _mediator
                .Setup(x => x.Send(It.IsAny<ProcessRejectedPaymentRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(processRejectedPaymentResponse);

            _mockBankApiClient
                .Setup(x => x.TransferFunds(It.IsAny<TransferFundsRequest>()))
                .ReturnsAsync(TransferFundsResponse.Error(422, _fixture.Create<TransferFundsErrorResponse>()));

            //act
            var res = await _createPaymentHandler.Handle(_fixture.Create<CreatePaymentRequest>());

            //assert
            res.StatusCode.Should().Be(StatusCodes.Status201Created);
            res.SuccessResponse.Status.Should().Be(PaymentStatus.Rejected);
            res.SuccessResponse.PaymentId.Should().Be(processRejectedPaymentResponse.Id);
        }

        [Test]
        public async Task Handle_BankResponseRejected_SendsProcessRejectedPaymentRequest()
        {
            //arrange
            var merchantId = _fixture.Create<string>();

            _merchantContext.Setup(x => x.GetMerchantId()).Returns(merchantId);

            var transferFundsErrorResponse = _fixture.Create<TransferFundsErrorResponse>();

            _mockBankApiClient
                .Setup(x => x.TransferFunds(It.IsAny<TransferFundsRequest>()))
                .ReturnsAsync(TransferFundsResponse.Error(StatusCodes.Status422UnprocessableEntity, transferFundsErrorResponse));

            var request = _fixture.Create<CreatePaymentRequest>();

            var expected = new ProcessRejectedPaymentRequest
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
                    Id = merchantId,
                },
                BankResponse = new ProcessRejectedPaymentRequest.BankPaymentResponse
                {
                    FailureReason = transferFundsErrorResponse.Code
                }
            };

            //act
            await _createPaymentHandler.Handle(request);

            //assert
            _mediator.Verify(x => x.Send(It.Is<ProcessRejectedPaymentRequest>(req => req.Should().BeEquivalentToBool(expected)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_BankResponseUnknownError_ReturnsBadGateway()
        {
            //arrange
            var transferFundsResponse = TransferFundsResponse.UnknownError(_fixture.Create<int>());

            _mockBankApiClient
                .Setup(x => x.TransferFunds(It.IsAny<TransferFundsRequest>()))
                .ReturnsAsync(transferFundsResponse);

            //act
            var res = await _createPaymentHandler.Handle(_fixture.Create<CreatePaymentRequest>());

            //assert
            res.StatusCode.Should().Be(StatusCodes.Status502BadGateway);
        }

        [Test]
        public async Task Handle_InvokesMockBankWithExpectedValues()
        {
            //arrange
            var createPaymentRequest = _fixture.Create<CreatePaymentRequest>();

            var expected = new TransferFundsRequest
            {
                Currency = createPaymentRequest.Currency,
                Amount = createPaymentRequest.Amount,
                Source = new TransferFundsRequest.PaymentSource
                {
                    Cvv = createPaymentRequest.Source.Cvv,
                    CardExpiry = createPaymentRequest.Source.CardExpiry,
                    CardNumber = createPaymentRequest.Source.CardNumber,
                },
                Recipient = new TransferFundsRequest.PaymentRecipient
                {
                    SortCode = createPaymentRequest.Recipient.SortCode,
                    AccountNumber = createPaymentRequest.Recipient.AccountNumber
                }
            };

            //act
            await _createPaymentHandler.Handle(createPaymentRequest);

            //assert
            _mockBankApiClient.Verify(x => x.TransferFunds(It.Is<TransferFundsRequest>(req => req.Should().BeEquivalentToBool(expected))), Times.Once);
        }
    }

    //TODO: move to shared lib
    public static class FluentAssertionExtensions
    {
        public static bool BeEquivalentToBool(this ObjectAssertions objectAssertions, object expected)
        {
            try
            {
                objectAssertions.Subject.Should().BeEquivalentTo(expected);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
