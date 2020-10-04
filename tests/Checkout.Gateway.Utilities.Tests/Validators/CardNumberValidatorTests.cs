using Checkout.Gateway.Utilities.Validators;
using FluentAssertions;
using NUnit.Framework;

namespace Checkout.Gateway.Utilities.Tests.Validators
{
    public class CardNumberValidatorTests
    {
        private CardNumberValidator _cardNumberValidator;

        [SetUp]
        public void SetUp()
        {
            // Sut instantiation
            _cardNumberValidator = new CardNumberValidator();
        }

        [TestCase("4000058260000005")]
        [TestCase("5403134279301138")]
        [TestCase("5169858453134022")]
        [TestCase("378796656824619")]
        [TestCase("5516407902202707")]
        [TestCase("6304877390394444")]
        [TestCase("6398333356008914")]
        public void IsValid_ValidCardNumber_ReturnsTrue(string cardNumber)
        {
            //act, assert
            _cardNumberValidator.IsValid(cardNumber).Should().BeTrue();
        }

        [TestCase("SS00058260000005")] //Non-numerical char
        [TestCase("540313427930")] //Invalid length
        [TestCase("5169858453134025")] //Invalid checksum
        [TestCase("asdasd")] //Nonsense
        public void IsValid_InvalidCardNumber_ReturnsFalse(string cardNumber)
        {
            //act, assert
            _cardNumberValidator.IsValid(cardNumber).Should().BeFalse();
        }
    }
}
