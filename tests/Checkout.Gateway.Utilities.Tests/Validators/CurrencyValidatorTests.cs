using AutoFixture;
using Checkout.Gateway.Utilities.Validators;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;

namespace Checkout.Gateway.Utilities.Tests.Validators
{
    [TestFixture]
    public class CurrencyValidatorTests
    {
        private MockRepository _mockRepository;
        private IFixture _fixture;

        private Mock<IOptionsMonitor<SupportedCurrencyProvider>> _options;

        private CurrencyValidator _currencyValidator;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _fixture = new Fixture();

            // Mock setup
            _options = _mockRepository.Create<IOptionsMonitor<SupportedCurrencyProvider>>();

            // Mock default
            SetupMockDefaults();

            // Sut instantiation
            _currencyValidator = new CurrencyValidator(
                _options.Object
            );
        }

        private void SetupMockDefaults()
        {
            _options.Setup(x => x.CurrentValue).Returns(_fixture.Create<SupportedCurrencyProvider>());
        }

        [Test]
        public void IsSupported_CurrencyIsSupported_ReturnsTrue()
        {
            //arrange
            var currency = _fixture.Create<string>();

            var supportedCurrencies = new List<string>
            {
                currency
            };

            _options.Setup(x => x.CurrentValue).Returns(new SupportedCurrencyProvider
            {
                SupportedCurrencies = supportedCurrencies
            });

            //act
            var res = _currencyValidator.IsSupported(currency);

            //assert
            res.Should().BeTrue();
        }

        [Test]
        public void IsSupported_CurrencyIsNotSupported_ReturnsFalse()
        {
            //arrange
            var supportedCurrencies = new List<string>();

            _options.Setup(x => x.CurrentValue).Returns(new SupportedCurrencyProvider
            {
                SupportedCurrencies = supportedCurrencies
            });

            //act
            var res = _currencyValidator.IsSupported(_fixture.Create<string>());

            //assert
            res.Should().BeFalse();
        }
    }
}
