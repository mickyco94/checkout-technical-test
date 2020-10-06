using AutoFixture;
using Checkout.Gateway.Data.Abstractions;
using Checkout.Gateway.Data.Models;
using Checkout.Gateway.Service.Commands.ProcessSuccessfulPayment;
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
    public class ProcessSuccessfulPaymentHandlerTests
    {
        private MockRepository _mockRepository;
        private IFixture _fixture;

        private Mock<IPaymentRecordCreator> _paymentRecordCreator;
        private Mock<IGuid> _guid;
        private Mock<IDateTime> _dateTime;

        private ProcessSuccessfulPaymentHandler _processSuccessfulPaymentHandler;
        private Mock<IEncrypter> _encrypter;
        private Mock<IMerchantEncryptionKeyGetter> _merchantKeys;

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
            _processSuccessfulPaymentHandler = new ProcessSuccessfulPaymentHandler(
                _merchantKeys.Object,
                _encrypter.Object,
                _paymentRecordCreator.Object,
                _guid.Object,
                _dateTime.Object
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

            var request = _fixture.Create<ProcessSuccessfulPaymentRequest>();

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
                Status = PaymentStatus.Succeeded,
                Amount = request.Amount,
                CreatedAt = dateTime,
                BankPaymentId = request.BankResponse.TransactionId,
                MerchantId = request.Merchant.Id
            };

            //act
            await _processSuccessfulPaymentHandler.Handle(request);

            //assert
            _paymentRecordCreator.Verify(x => x.Add(It.Is<PaymentRecord>(record => record.Should().BeEquivalentToBool(paymentRecord))), Times.Once);
        }

        [Test]
        public async Task Handle_ReturnsIdOfCreatedPaymentRecord()
        {
            //arrange
            var guid = _fixture.Create<Guid>();

            _guid.Setup(x => x.NewGuid()).Returns(guid);

            var expected = new ProcessSuccessfulPaymentResponse
            {
                Id = guid.ToString()
            };

            //act
            var res = await _processSuccessfulPaymentHandler.Handle(_fixture.Create<ProcessSuccessfulPaymentRequest>());

            //assert
            res.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task Handle_GetsEncryptionKeyForRequestMerchant()
        {
            //arrange
            var request = _fixture.Create<ProcessSuccessfulPaymentRequest>();

            //act
            await _processSuccessfulPaymentHandler.Handle(request);

            //assert
            _merchantKeys.Verify(x => x.Key(request.Merchant.Id), Times.Once);
        }

        [Test]
        public async Task Handle_EncryptsUserDataUsingMerchantKey()
        {
            //arrange
            var request = _fixture.Create<ProcessSuccessfulPaymentRequest>();

            var key = _fixture.Create<byte[]>();

            _merchantKeys.Setup(x => x.Key(It.IsAny<string>())).Returns(key);

            //act
            await _processSuccessfulPaymentHandler.Handle(request);

            //assert
            _encrypter.Verify(x => x.EncryptUtf8(request.Source.Cvv, key), Times.Once);
            _encrypter.Verify(x => x.EncryptUtf8(request.Source.CardExpiry, key), Times.Once);
            _encrypter.Verify(x => x.EncryptUtf8(request.Source.CardNumber, key), Times.Once);
            _encrypter.Verify(x => x.EncryptUtf8(request.Recipient.AccountNumber, key), Times.Once);
            _encrypter.Verify(x => x.EncryptUtf8(request.Recipient.SortCode, key), Times.Once);
        }
    }
}
