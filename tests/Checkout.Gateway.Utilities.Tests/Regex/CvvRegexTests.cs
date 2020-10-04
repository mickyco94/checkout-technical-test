using Checkout.Gateway.Utilities.Regex;
using FluentAssertions;
using NUnit.Framework;

namespace Checkout.Gateway.Utilities.Tests.Regex
{
    public class CvvRegexTests
    {
        private CvvRegex _cvvRegex;

        [SetUp]
        public void SetUp()
        {
            // Sut instantiation
            _cvvRegex = new CvvRegex();
        }

        [TestCase("123")]
        [TestCase("1234")]
        public void IsValid_ValidCardNumber_ReturnsTrue(string cvv)
        {
            //act, assert
            _cvvRegex.IsMatch(cvv).Should().BeTrue();
        }

        [TestCase("")]
        [TestCase("sdasd")]
        [TestCase("123S")]
        [TestCase("!23")]
        [TestCase("12SS")]
        public void IsValid_InvalidCardNumber_ReturnsFalse(string cvv)
        {
            //act, assert
            _cvvRegex.IsMatch(cvv).Should().BeFalse();
        }
    }
}
