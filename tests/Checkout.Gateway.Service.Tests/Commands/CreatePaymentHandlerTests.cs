using AutoFixture;
using Checkout.Gateway.Service.Commands.CreatePayment;
using Checkout.Gateway.Utilities;
using FluentAssertions;
using FluentAssertions.Primitives;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MockBank.API.Client;
using MockBank.API.Client.Models;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Checkout.Gateway.Service.Tests.Commands
{
    [TestFixture]
    public class CreatePaymentHandlerTests
    {
        private MockRepository _mockRepository;
        private IFixture _fixture;

        private CreatePaymentHandler _createPaymentHandler;

        private Mock<IGuid> _guid;
        private Mock<ILogger<CreatePaymentHandler>> _logger;
        private Mock<IMockBankApiClient> _mockBankApiClient;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _fixture = new Fixture();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));

            _guid = _mockRepository.Create<IGuid>();
            _logger = _mockRepository.Create<ILogger<CreatePaymentHandler>>(MockBehavior.Loose);
            _mockBankApiClient = _mockRepository.Create<IMockBankApiClient>();

            // Mock default
            SetupMockDefaults();

            // Sut instantiation
            _createPaymentHandler = new CreatePaymentHandler(_logger.Object,
                _mockBankApiClient.Object,
                _guid.Object);
        }

        private void SetupMockDefaults()
        {
            _guid
                .Setup(x => x.NewGuid())
                .Returns(Guid.NewGuid);

            _mockBankApiClient
                .Setup(x => x.TransferFunds(It.IsAny<TransferFundsRequest>()))
                .ReturnsAsync(TransferFundsResponse.Successful(_fixture.Create<TransferFundsSuccessfulResponse>()));
        }

        [Test]
        public async Task Handle_BankResponseSuccessful_ReturnsSuccessStatus()
        {
            //arrange
            _mockBankApiClient
                .Setup(x => x.TransferFunds(It.IsAny<TransferFundsRequest>()))
                .ReturnsAsync(TransferFundsResponse.Successful(_fixture.Create<TransferFundsSuccessfulResponse>()));

            //act
            var res = await _createPaymentHandler.Handle(_fixture.Create<CreatePaymentRequest>());

            //assert
            res.StatusCode.Should().Be(StatusCodes.Status201Created);
            res.SuccessResponse.Status.Should().Be(PaymentStatus.Succeeded);
        }

        [Test]
        public async Task Handle_BankResponseRejected_ReturnsRejectedStatus()
        {
            //arrange
            _mockBankApiClient
                .Setup(x => x.TransferFunds(It.IsAny<TransferFundsRequest>()))
                .ReturnsAsync(TransferFundsResponse.Error(422, _fixture.Create<TransferFundsErrorResponse>()));

            //act
            var res = await _createPaymentHandler.Handle(_fixture.Create<CreatePaymentRequest>());

            //assert
            res.StatusCode.Should().Be(StatusCodes.Status201Created);
            res.SuccessResponse.Status.Should().Be(PaymentStatus.Rejected);
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
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
