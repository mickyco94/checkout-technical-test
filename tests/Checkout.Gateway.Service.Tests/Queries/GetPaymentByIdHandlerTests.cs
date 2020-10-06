using AutoFixture;
using Checkout.Gateway.Data.Abstractions;
using Checkout.Gateway.Data.Models;
using Checkout.Gateway.Service.Queries;
using Checkout.Gateway.Utilities;
using Checkout.Gateway.Utilities.Encryption;
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
        private Mock<IMerchantEncryptionKeyGetter> _merchantEncryptionKeyGetter;
        private Mock<IDecrypter> _decrypter;

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
            _merchantEncryptionKeyGetter = _mockRepository.Create<IMerchantEncryptionKeyGetter>();
            _decrypter = _mockRepository.Create<IDecrypter>();

            // Mock default
            SetupMockDefaults();

            // Sut instantiation
            _getPaymentByIdHandler = new GetPaymentByIdHandler(
                _merchantEncryptionKeyGetter.Object,
                _decrypter.Object,
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

            _merchantEncryptionKeyGetter
                .Setup(x => x.Key(It.IsAny<string>()))
                .Returns(_fixture.Create<byte[]>());

            _decrypter
                .Setup(x => x.DecryptUtf8(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(_fixture.Create<string>());
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

            var decryptedCvv = _fixture.Create<string>();
            var decryptedCardNumber = _fixture.Create<string>();
            var decryptedCardExpiry = _fixture.Create<string>();

            var decryptedSortCode = _fixture.Create<string>();
            var decryptedAccountNumber = _fixture.Create<string>();

            _decrypter
                .Setup(x => x.DecryptUtf8(paymentRecord.Source.CvvEncrypted, It.IsAny<byte[]>()))
                .Returns(decryptedCvv);

            _decrypter
                .Setup(x => x.DecryptUtf8(paymentRecord.Source.CardNumberEncrypted, It.IsAny<byte[]>()))
                .Returns(decryptedCardNumber);

            _decrypter
                .Setup(x => x.DecryptUtf8(paymentRecord.Source.CardExpiryEncrypted, It.IsAny<byte[]>()))
                .Returns(decryptedCardExpiry);

            _decrypter
                .Setup(x => x.DecryptUtf8(paymentRecord.Recipient.AccountNumberEncrypted, It.IsAny<byte[]>()))
                .Returns(decryptedAccountNumber);

            _decrypter
                .Setup(x => x.DecryptUtf8(paymentRecord.Recipient.SortCodeEncrypted, It.IsAny<byte[]>()))
                .Returns(decryptedSortCode);

            var expected = new GetPaymentByIdResponse
            {
                Id = paymentRecord.Id,
                Source = new GetPaymentByIdResponse.PaymentSource
                {
                    Cvv = decryptedCvv.Mask(3),
                    CardExpiry = decryptedCardExpiry,
                    CardNumber = decryptedCardNumber.Mask(12),
                },
                Recipient = new GetPaymentByIdResponse.PaymentRecipient
                {
                    SortCode = decryptedSortCode.Mask(4),
                    AccountNumber = decryptedAccountNumber.Mask(6, 0)
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

        [Test]
        public async Task Handle_DecryptsUserDataUsingMerchantKey()
        {
            //arrange
            var paymentRecord = _fixture.Create<PaymentRecord>();

            var paymentRecords = new List<PaymentRecord>
            {
                paymentRecord
            };

            _merchantContext
                .Setup(x => x.GetMerchantId())
                .Returns(paymentRecord.MerchantId);

            _paymentRecordReader
                .Setup(x => x.PaymentRecords)
                .Returns(paymentRecords.AsQueryable);

            var key = _fixture.Create<byte[]>();

            _merchantEncryptionKeyGetter.Setup(x => x.Key(It.IsAny<string>())).Returns(key);

            //act
            await _getPaymentByIdHandler.Handle(new GetPaymentByIdRequest
            {
                Id = paymentRecord.Id
            });

            //assert
            _decrypter.Verify(x => x.DecryptUtf8(paymentRecord.Source.CvvEncrypted, key), Times.Once);
            _decrypter.Verify(x => x.DecryptUtf8(paymentRecord.Source.CardNumberEncrypted, key), Times.Once);
            _decrypter.Verify(x => x.DecryptUtf8(paymentRecord.Source.CardExpiryEncrypted, key), Times.Once);
            _decrypter.Verify(x => x.DecryptUtf8(paymentRecord.Recipient.SortCodeEncrypted, key), Times.Once);
            _decrypter.Verify(x => x.DecryptUtf8(paymentRecord.Recipient.AccountNumberEncrypted, key), Times.Once);
        }
    }
}
