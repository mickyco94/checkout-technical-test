using AutoFixture;
using Checkout.Gateway.Data.Abstractions;
using Checkout.Gateway.Data.Models;
using Checkout.Gateway.Service.Commands.ProcessRejectedPayment;
using Checkout.Gateway.Utilities;
using Checkout.Gateway.Utilities.Encryption;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Checkout.Gateway.Service.Tests.Commands
{
    [TestFixture]
    public class ProcessRejectedPaymentHandlerTests
    {
        private MockRepository _mockRepository;
        private IFixture _fixture;

        private Mock<IPaymentRecordCreator> _paymentRecordCreator;
        private Mock<IGuid> _guid;
        private Mock<IDateTime> _dateTime;
        private Mock<IEncrypter> _encrypter;
        private Mock<IMerchantEncryptionKeyGetter> _merchantKeys;

        private ProcessRejectedPaymentHandler _processRejectedPaymentHandler;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _fixture = new Fixture();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));

            // Mock setup
            _paymentRecordCreator = _mockRepository.Create<IPaymentRecordCreator>();
            _guid = _mockRepository.Create<IGuid>();
            _dateTime = _mockRepository.Create<IDateTime>();
            _encrypter = _mockRepository.Create<IEncrypter>();
            _merchantKeys = _mockRepository.Create<IMerchantEncryptionKeyGetter>();

            // Mock default
            SetupMockDefaults();

            // Sut instantiation
            _processRejectedPaymentHandler = new ProcessRejectedPaymentHandler(
                _encrypter.Object,
                _paymentRecordCreator.Object,
                _guid.Object,
                _dateTime.Object,
                _merchantKeys.Object
            );
        }

        private void SetupMockDefaults()
        {
            _paymentRecordCreator.Setup(x => x.Add(It.IsAny<PaymentRecord>()));

            _guid.Setup(x => x.NewGuid()).Returns(Guid.NewGuid);

            _dateTime.Setup(x => x.UtcNow()).Returns(_fixture.Create<DateTime>());

            _encrypter.Setup(x => x.EncryptUtf8(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(_fixture.Create<string>());

            _merchantKeys.Setup(x => x.Key(It.IsAny<string>())).Returns(_fixture.Create<byte[]>());
        }

        [Test]
        public async Task Handle_CreatesPaymentRecordWithExpectedValues()
        {
            //arrange
            var guid = _fixture.Create<Guid>();

            _guid.Setup(x => x.NewGuid()).Returns(guid);

            var dateTime = _fixture.Create<DateTime>();

            _dateTime.Setup(x => x.UtcNow()).Returns(dateTime);

            var request = _fixture.Create<ProcessRejectedPaymentRequest>();

            var encryptedCardExpiry = _fixture.Create<string>();

            _encrypter
                .Setup(x => x.EncryptUtf8(request.Source.CardExpiry, It.IsAny<byte[]>()))
                .Returns(encryptedCardExpiry);

            var encryptedCardNumber = _fixture.Create<string>();
            _encrypter
                .Setup(x => x.EncryptUtf8(request.Source.CardNumber, It.IsAny<byte[]>()))
                .Returns(encryptedCardNumber);

            var encryptedCardCvv = _fixture.Create<string>();
            _encrypter
                .Setup(x => x.EncryptUtf8(request.Source.Cvv, It.IsAny<byte[]>()))
                .Returns(encryptedCardCvv);

            var encryptedAccNumber = _fixture.Create<string>();
            _encrypter
                .Setup(x => x.EncryptUtf8(request.Recipient.AccountNumber, It.IsAny<byte[]>()))
                .Returns(encryptedAccNumber);

            var encryptedSortCode = _fixture.Create<string>();
            _encrypter
                .Setup(x => x.EncryptUtf8(request.Recipient.SortCode, It.IsAny<byte[]>()))
                .Returns(encryptedSortCode);

            var paymentRecord = new PaymentRecord
            {
                Id = guid.ToString(),
                Source = new PaymentRecord.PaymentSource
                {
                    CardExpiryEncrypted = encryptedCardExpiry,
                    CardNumberEncrypted = encryptedCardNumber,
                    CvvEncrypted = encryptedCardCvv
                },
                Recipient = new PaymentRecord.PaymentRecipient
                {
                    AccountNumberEncrypted = encryptedAccNumber,
                    SortCodeEncrypted = encryptedSortCode,
                },
                Currency = request.Currency,
                Status = PaymentStatus.Rejected,
                Amount = request.Amount,
                CreatedAt = dateTime,
                BankPaymentId = null,
                FailureReason = request.BankResponse.FailureReason,
                MerchantId = request.Merchant.Id
            };

            //act
            await _processRejectedPaymentHandler.Handle(request);

            //assert
            _paymentRecordCreator.Verify(x => x.Add(It.Is<PaymentRecord>(record => record.Should().BeEquivalentToBool(paymentRecord))), Times.Once);
        }

        [Test]
        public async Task Handle_ReturnsIdOfCreatedPaymentRecord()
        {
            //arrange
            var guid = _fixture.Create<Guid>();

            _guid.Setup(x => x.NewGuid()).Returns(guid);

            var expected = new ProcessRejectedPaymentResponse
            {
                Id = guid.ToString()
            };

            //act
            var res = await _processRejectedPaymentHandler.Handle(_fixture.Create<ProcessRejectedPaymentRequest>());

            //assert
            res.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task Handle_GetsEncryptionKeyForRequestMerchant()
        {
            //arrange
            var request = _fixture.Create<ProcessRejectedPaymentRequest>();

            //act
            await _processRejectedPaymentHandler.Handle(request);

            //assert
            _merchantKeys.Verify(x => x.Key(request.Merchant.Id), Times.Once);
        }

        [Test]
        public async Task Handle_EncryptsUserDataUsingMerchantKey()
        {
            //arrange
            var request = _fixture.Create<ProcessRejectedPaymentRequest>();

            var key = _fixture.Create<byte[]>();

            _merchantKeys.Setup(x => x.Key(It.IsAny<string>())).Returns(key);

            //act
            await _processRejectedPaymentHandler.Handle(request);

            //assert
            _encrypter.Verify(x => x.EncryptUtf8(request.Source.Cvv, key), Times.Once);
            _encrypter.Verify(x => x.EncryptUtf8(request.Source.CardExpiry, key), Times.Once);
            _encrypter.Verify(x => x.EncryptUtf8(request.Source.CardNumber, key), Times.Once);
            _encrypter.Verify(x => x.EncryptUtf8(request.Recipient.AccountNumber, key), Times.Once);
            _encrypter.Verify(x => x.EncryptUtf8(request.Recipient.SortCode, key), Times.Once);
        }
    }
}
