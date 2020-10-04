using AutoFixture;
using Checkout.Gateway.Service.Commands.CreatePayment;
using Checkout.Gateway.Utilities.Regex;
using Checkout.Gateway.Utilities.Validators;
using FluentValidation;
using FluentValidation.TestHelper;
using Moq;
using NUnit.Framework;

namespace Checkout.Gateway.Service.Tests.Commands
{
    [TestFixture]
    public class CreatePaymentValidatorTests
    {
        private MockRepository _mockRepository;
        private IFixture _fixture;

        private CreatePaymentValidator _createPaymentValidator;

        private Mock<ICvvRegex> _cvvRegex;
        private Mock<ICardExpiryValidator> _cardExpiryValidator;
        private Mock<ICurrencyValidator> _currencyValidator;
        private Mock<ICardNumberValidator> _cardNumberValidator;
        private Mock<IAccountNumberRegex> _accountNumberRegex;
        private Mock<ISortCodeRegex> _sortCodeRegex;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _fixture = new Fixture();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));

            //Mock setup
            _cvvRegex = _mockRepository.Create<ICvvRegex>();
            _cardExpiryValidator = _mockRepository.Create<ICardExpiryValidator>();
            _currencyValidator = _mockRepository.Create<ICurrencyValidator>();
            _cardNumberValidator = _mockRepository.Create<ICardNumberValidator>();

            _accountNumberRegex = _mockRepository.Create<IAccountNumberRegex>();
            _sortCodeRegex = _mockRepository.Create<ISortCodeRegex>();

            // Mock default
            SetupMockDefaults();

            // Sut instantiation
            _createPaymentValidator = new CreatePaymentValidator(
                _currencyValidator.Object,
                _cvvRegex.Object,
                _sortCodeRegex.Object,
                _accountNumberRegex.Object,
                _cardExpiryValidator.Object,
                _cardNumberValidator.Object);
        }

        private void SetupMockDefaults()
        {
            _accountNumberRegex.Setup(x => x.IsMatch(It.IsAny<string>())).Returns(true);
            _sortCodeRegex.Setup(x => x.IsMatch(It.IsAny<string>())).Returns(true);
            _cvvRegex.Setup(x => x.IsMatch(It.IsAny<string>())).Returns(true);

            _cardExpiryValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            _cardNumberValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            _cardExpiryValidator.Setup(x => x.IsExpired(It.IsAny<string>())).Returns(false);
            _currencyValidator.Setup(x => x.IsSupported(It.IsAny<string>())).Returns(true);
        }

        [Test]
        public void ValidateAsync_PaymentSourceNull_ShouldHaveValidationError()
        {
            //arrange
            var request = _fixture.Create<CreatePaymentRequest>();
            request.Source = null;

            //act, assert
            _createPaymentValidator.ShouldHaveValidationErrorFor(a => a.Source, request)
                .WithErrorCode("ERR_SOURCE")
                .WithErrorMessage("No payment source provided");
        }

        [Test]
        public void ValidateAsync_PaymentRecipientNull_ShouldHaveValidationError()
        {
            //arrange
            var request = _fixture.Create<CreatePaymentRequest>();
            request.Recipient = null;

            //act, assert
            _createPaymentValidator.ShouldHaveValidationErrorFor(a => a.Recipient, request)
                .WithErrorCode("ERR_RECIPIENT")
                .WithErrorMessage("No payment recipient provided");
        }

        [Test]
        public void ValidateAsync_CurrencyNotSupported_ShouldHaveValidationError()
        {
            //arrange
            _currencyValidator.Setup(x => x.IsSupported(It.IsAny<string>())).Returns(false);

            //act, assert
            _createPaymentValidator.ShouldHaveValidationErrorFor(a => a.Currency, _fixture.Create<CreatePaymentRequest>())
                .WithErrorCode("ERR_CURRENCY")
                .WithErrorMessage("Specified currency is not supported");
        }

        [Test]
        public void ValidateAsync_CurrencySupported_ShouldNotHaveValidationError()
        {
            //arrange
            _currencyValidator.Setup(x => x.IsSupported(It.IsAny<string>())).Returns(true);

            //act, assert
            _createPaymentValidator.ShouldNotHaveValidationErrorFor(a => a.Currency, _fixture.Create<CreatePaymentRequest>());
        }

        [Test]
        public void ValidateAsync_ValidatesCurrencySupportedUsingRequestObject()
        {
            //arrange
            var request = _fixture.Create<CreatePaymentRequest>();

            //act
            _createPaymentValidator.Validate(request);

            //assert
            _currencyValidator.Verify(x => x.IsSupported(request.Currency), Times.Once);
        }

        [Test]
        public void ValidateAsync_InvalidCvv_ShouldHaveValidationError()
        {
            //arrange
            _cvvRegex.Setup(x => x.IsMatch(It.IsAny<string>())).Returns(false);

            //act, assert
            _createPaymentValidator.ShouldHaveValidationErrorFor(a => a.Source.Cvv, _fixture.Create<CreatePaymentRequest>())
                .WithErrorCode("ERR_CVV")
                .WithErrorMessage("Invalid CVV");
        }

        [Test]
        public void ValidateAsync_ValidCvv_ShouldNotHaveValidationError()
        {
            //arrange
            _cvvRegex.Setup(x => x.IsMatch(It.IsAny<string>())).Returns(true);

            //act, assert
            _createPaymentValidator.ShouldNotHaveValidationErrorFor(a => a.Source.Cvv, _fixture.Create<CreatePaymentRequest>());
        }

        [Test]
        public void ValidateAsync_ValidatesRequestCvvUsingRegex()
        {
            //arrange
            var request = _fixture.Create<CreatePaymentRequest>();

            //act
            _createPaymentValidator.Validate(request);

            //assert
            _cvvRegex.Verify(x => x.IsMatch(request.Source.Cvv), Times.Once);
        }

        [Test]
        public void ValidateAsync_InvalidCardNumber_ShouldHaveValidationError()
        {
            //arrange
            _cardNumberValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(false);

            //act, assert
            _createPaymentValidator.ShouldHaveValidationErrorFor(a => a.Source.CardNumber, _fixture.Create<CreatePaymentRequest>())
                .WithErrorCode("ERR_CARD_NO")
                .WithErrorMessage("Invalid card number");
        }

        [Test]
        public void ValidateAsync_ValidCardNumber_ShouldNotHaveValidationError()
        {
            //arrange
            _cardNumberValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);

            //act, assert
            _createPaymentValidator.ShouldNotHaveValidationErrorFor(a => a.Source.Cvv, _fixture.Create<CreatePaymentRequest>());
        }

        [Test]
        public void ValidateAsync_ValidatesRequestCardNumberUsingCardNumberValidator()
        {
            //arrange
            var request = _fixture.Create<CreatePaymentRequest>();

            //act
            _createPaymentValidator.Validate(request);

            //assert
            _cardNumberValidator.Verify(x => x.IsValid(request.Source.CardNumber), Times.Once);
        }

        [Test]
        public void ValidateAsync_InvalidCardExpiry_ShouldHaveValidationError()
        {
            //arrange
            _cardExpiryValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(false);

            //act, assert
            _createPaymentValidator.ShouldHaveValidationErrorFor(a => a.Source.CardExpiry, _fixture.Create<CreatePaymentRequest>())
                .WithErrorCode("ERR_CARD_EXP_FORMAT")
                .WithErrorMessage("Invalid source card expiry");
        }

        [Test]
        public void ValidateAsync_ValidCardExpiryFormat_CardExpired_ShouldHaveValidationError()
        {
            //arrange
            _cardExpiryValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            _cardExpiryValidator.Setup(x => x.IsExpired(It.IsAny<string>())).Returns(true);

            //act, assert
            _createPaymentValidator.ShouldHaveValidationErrorFor(a => a.Source.CardExpiry, _fixture.Create<CreatePaymentRequest>())
                .WithErrorCode("ERR_CARD_EXP_EXP")
                .WithErrorMessage("Source card has expired");
        }

        [Test]
        public void ValidateAsync_ValidCardExpiry_CardNotExpired_ShouldNotHaveValidationError()
        {
            //arrange
            _cardExpiryValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            _cardExpiryValidator.Setup(x => x.IsExpired(It.IsAny<string>())).Returns(false);

            //act, assert
            _createPaymentValidator.ShouldNotHaveValidationErrorFor(a => a.Source.CardExpiry, _fixture.Create<CreatePaymentRequest>());
        }

        [Test]
        public void ValidateAsync_ValidatesRequestCardExpiryUsingCardExpiryValidator()
        {
            //arrange
            var request = _fixture.Create<CreatePaymentRequest>();

            //act
            _createPaymentValidator.Validate(request);

            //assert
            _cardExpiryValidator.Verify(x => x.IsValid(request.Source.CardExpiry), Times.Once);
        }

        [Test]
        public void ValidateAsync_ValidCardExpiryFormat_ValidatesRequestCardExpiryUsingRequestObject()
        {
            //arrange
            var request = _fixture.Create<CreatePaymentRequest>();
            _cardExpiryValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);

            //act
            _createPaymentValidator.Validate(request);

            //assert
            _cardExpiryValidator.Verify(x => x.IsExpired(request.Source.CardExpiry), Times.Once);
        }
    }
}
