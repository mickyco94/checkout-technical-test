using AutoFixture;
using Checkout.Gateway.Data.Abstractions;
using Checkout.Gateway.Data.Models;
using Checkout.Gateway.Service.Queries;
using Checkout.Gateway.Utilities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Checkout.Gateway.Service.Tests.Queries
{
    [TestFixture]
    public class GetPaymentByIdHandlerTests
    {
        private MockRepository _mockRepository;
        private IFixture _fixture;

        private Mock<IPaymentRecordReader> _paymentRecordReader;
        private Mock<IMerchantContext> _merchantContext;

        private GetPaymentByIdHandler _getPaymentByIdHandler;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _fixture = new Fixture();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));

            // Mock setup
            _paymentRecordReader = _mockRepository.Create<IPaymentRecordReader>();
            _merchantContext = _mockRepository.Create<IMerchantContext>();

            // Mock default
            SetupMockDefaults();

            // Sut instantiation
            _getPaymentByIdHandler = new GetPaymentByIdHandler(
                _paymentRecordReader.Object,
                _merchantContext.Object
            );
        }

        private void SetupMockDefaults()
        {
            _merchantContext
                .Setup(x => x.GetMerchantId())
                .Returns(_fixture.Create<string>());

            _paymentRecordReader
                .Setup(x => x.PaymentRecords)
                .Returns(new List<PaymentRecord>().AsQueryable());
        }

        [Test]
        public async Task Handle_IdNotFound_Returns404()
        {
            //arrange
            var errorResponse = ApiResponse<GetPaymentByIdResponse>.Fail(StatusCodes.Status404NotFound,
                "NOT_FOUND",
                "Payment record not found");

            //act
            var res = await _getPaymentByIdHandler.Handle(_fixture.Create<GetPaymentByIdRequest>());

            //assert
            res.Should().BeEquivalentTo(errorResponse);
        }

        [Test]
        public async Task Handle_CorrectId_MerchantIdMismatch_Returns404()
        {
            //arrange
            var errorResponse = ApiResponse<GetPaymentByIdResponse>.Fail(StatusCodes.Status404NotFound,
                "NOT_FOUND",
                "Payment record not found");

            var paymentRecord = _fixture.Create<PaymentRecord>();

            var paymentRecords = new List<PaymentRecord>
            {
                paymentRecord
            };

            _paymentRecordReader
                .Setup(x => x.PaymentRecords)
                .Returns(paymentRecords.AsQueryable);

            var request = new GetPaymentByIdRequest
            {
                Id = paymentRecord.Id
            };

            //act
            var res = await _getPaymentByIdHandler.Handle(request);

            //assert
            res.Should().BeEquivalentTo(errorResponse);
        }

        [Test]
        public async Task Handle_CorrectId_CorrectId_ReturnsSuccessfulResult()
        {
            //arrange
            var paymentRecord = _fixture.Create<PaymentRecord>();

            var paymentRecords = new List<PaymentRecord>
            {
                paymentRecord
            };

            _paymentRecordReader
                .Setup(x => x.PaymentRecords)
                .Returns(paymentRecords.AsQueryable);

            _merchantContext
                .Setup(x => x.GetMerchantId())
                .Returns(paymentRecord.MerchantId);

            var request = new GetPaymentByIdRequest
            {
                Id = paymentRecord.Id
            };

            var expected = new GetPaymentByIdResponse
            {
                Id = paymentRecord.Id,
                Source = new GetPaymentByIdResponse.PaymentSource
                {
                    Cvv = paymentRecord.Source.CvvEncrypted.Mask(3, 0),
                    CardExpiry = paymentRecord.Source.CardExpiryEncrypted,
                    CardNumber = paymentRecord.Source.CardNumberEncrypted.Mask(12),
                },
                Recipient = new GetPaymentByIdResponse.PaymentRecipient
                {
                    SortCode = paymentRecord.Recipient.SortCodeEncrypted.Mask(4),
                    AccountNumber = paymentRecord.Recipient.AccountNumberEncrypted.Mask(6, 0)
                },
                Details = string.IsNullOrEmpty(paymentRecord.FailureReason)
                    ? null
                    : new GetPaymentByIdResponse.BankResponse
                    {
                        FailureReason = paymentRecord.FailureReason
                    },
                Amount = paymentRecord.Amount,
                Status = paymentRecord.Status,
                CreatedAt = paymentRecord.CreatedAt,
                Currency = paymentRecord.Currency,
            };

            //act
            var res = await _getPaymentByIdHandler.Handle(request);

            //assert
            res.SuccessResponse.Should().BeEquivalentTo(expected);
            res.StatusCode.Should().Be(StatusCodes.Status200OK);
        }
    }
}
