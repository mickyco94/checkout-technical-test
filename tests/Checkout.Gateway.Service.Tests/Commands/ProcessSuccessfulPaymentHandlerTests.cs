﻿using AutoFixture;
using Checkout.Gateway.Data.Abstractions;
using Checkout.Gateway.Data.Models;
using Checkout.Gateway.Service.Commands.ProcessSuccessfulPayment;
using Checkout.Gateway.Utilities;
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

            // Mock default
            SetupMockDefaults();

            // Sut instantiation
            _processSuccessfulPaymentHandler = new ProcessSuccessfulPaymentHandler(
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

            var paymentRecord = new PaymentRecord
            {
                Id = guid.ToString(),
                Source = new PaymentRecord.PaymentSource
                {
                    CardExpiryEncrypted = request.Source.CardExpiry,
                    CardNumberEncrypted = request.Source.CardNumber,
                    CvvEncrypted = request.Source.Cvv
                },
                Recipient = new PaymentRecord.PaymentRecipient
                {
                    AccountNumberEncrypted = request.Recipient.AccountNumber,
                    SortCodeEncrypted = request.Recipient.SortCode,
                },
                Currency = request.Currency,
                Status = PaymentStatus.Succeeded,
                Amount = request.Amount,
                CreatedAt = dateTime,
                BankPaymentId = request.BankResponse.TransactionId,
                MerchantId = request.Merchant.Id
            };

            //act
            await _processSuccessfulPaymentHandler.Handle(new ProcessSuccessfulPaymentRequest());

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
            var res = await _processSuccessfulPaymentHandler.Handle(new ProcessSuccessfulPaymentRequest());

            //assert
            res.Should().BeEquivalentTo(expected);
        }
    }
}