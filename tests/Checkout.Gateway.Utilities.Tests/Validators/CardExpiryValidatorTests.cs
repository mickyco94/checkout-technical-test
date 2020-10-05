using AutoFixture;
using Checkout.Gateway.Utilities.Validators;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;

namespace Checkout.Gateway.Utilities.Tests.Validators
{
    [TestFixture]
    public class CardExpiryValidatorTests
    {
        private MockRepository _mockRepository;
        private IFixture _fixture;

        private Mock<IDateTime> _dateTime;

        private CardExpiryValidator _cardExpiryValidator;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _fixture = new Fixture();

            // Mock setup
            _dateTime = _mockRepository.Create<IDateTime>();

            // Mock default
            SetupMockDefaults();

            // Sut instantiation
            _cardExpiryValidator = new CardExpiryValidator(
                _dateTime.Object
            );
        }

        private void SetupMockDefaults()
        {
            _dateTime.Setup(x => x.UtcNow()).Returns(_fixture.Create<DateTime>());
        }

        [TestCase("01/2001")]
        [TestCase("02/2001")]
        [TestCase("03/2001")]
        [TestCase("04/2001")]
        [TestCase("05/2001")]
        [TestCase("06/2001")]
        [TestCase("07/2001")]
        [TestCase("08/2001")]
        [TestCase("09/2001")]
        [TestCase("10/2001")]
        [TestCase("11/2001")]
        [TestCase("12/2001")]
        public void IsValid_ValidFormat_ReturnsTrue(string input)
        {
            _cardExpiryValidator.IsValid(input).Should().BeTrue();
        }

        [Test]
        public void IsValid_ValidFormat_ReturnsTrue()
        {
            //arrange
            var dateTime = _fixture.Create<DateTime>();

            var dateTimeAsExpiryDate = dateTime.ToString("MM/yyyy");

            //act, assert
            _cardExpiryValidator.IsValid(dateTimeAsExpiryDate).Should().BeTrue();
        }

        [TestCase("")]
        [TestCase("dsadsa")]
        [TestCase("$213")]
        [TestCase("2020-10-04T15:31:52+0000")]
        [TestCase("Sun, 04 Oct 2020 15:31:52 +0000")]
        [TestCase("1601825512")]
        public void IsValid_InvalidFormat_ReturnsFalse(string input)
        {
            _cardExpiryValidator.IsValid(input).Should().BeFalse();
        }

        [Test]
        public void IsExpired_CurrentTimeIsDuringMonthOfExpiration_ReturnsFalse()
        {
            //arrange
            var expiryDate = _fixture.Create<DateTime>();

            var rand = new Random();

            var randomDayInMonth = rand.Next(1, DateTime.DaysInMonth(expiryDate.Year, expiryDate.Month));

            var currentTime = new DateTime(expiryDate.Year, expiryDate.Month, randomDayInMonth);

            _dateTime.Setup(x => x.UtcNow()).Returns(currentTime);

            var expiryDateAsString = expiryDate.ToString("MM/yyyy");

            //act, assert
            _cardExpiryValidator.IsExpired(expiryDateAsString).Should().BeFalse();
        }

        [Test]
        public void IsExpired_CurrentTimeIsBeforeMonthOfExpiration_ReturnsFalse()
        {
            //arrange
            long RandomNegativeValue() => -(_fixture.Create<uint>());

            var expiryDate = _fixture.Create<DateTime>();

            var currentTime = new DateTime(expiryDate.Year, expiryDate.Month, 1).AddTicks(RandomNegativeValue());

            _dateTime.Setup(x => x.UtcNow()).Returns(currentTime);

            var expiryDateAsString = expiryDate.ToString("MM/yyyy");

            //act, assert
            _cardExpiryValidator.IsExpired(expiryDateAsString).Should().BeFalse();
        }

        [Test]
        public void IsExpired_CurrentTimeAfterActualExpiration_ReturnsTrue()
        {
            //arrange
            var expiryDate = _fixture.Create<DateTime>();

            var currentTime = expiryDate.EndOfMonth().AddTicks(_fixture.Create<uint>());

            _dateTime.Setup(x => x.UtcNow()).Returns(currentTime);

            var expiryDateAsString = expiryDate.ToString("MM/yyyy");

            //act, assert
            _cardExpiryValidator.IsExpired(expiryDateAsString).Should().BeTrue();
        }
    }
}
