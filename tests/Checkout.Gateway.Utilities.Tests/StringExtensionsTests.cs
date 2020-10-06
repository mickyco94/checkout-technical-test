using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace Checkout.Gateway.Utilities.Tests
{
    [TestFixture]
    public class StringExtensionsTests
    {
        private IFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
        }

        //These first two tests I think are a little more exhaustive but are more difficult to understand at a glance
        //Usually I would ask for another developers input on which one they find easiest to understand. If no input was available then I'd defer to omitting
        //the first two tests in favour of the last 3.

        [TestCase(1, 2)]
        [TestCase(10, 12)]
        [TestCase(5, 10)]
        [TestCase(0, 5)]
        public void Mask_MaskStartIsN_LeavesFirstNCharactersAsOriginal(int start, int length)
        {
            //arrange
            var source = CreateStringOfLength(length);

            var expected = source.Substring(0, start);

            //act
            var actual = source.Mask(length - start, start).Substring(0, start);

            //assert
            actual.Should().Be(expected);
        }

        [TestCase(1, 2, 'X')]
        [TestCase(10, 12, '0')]
        [TestCase(5, 10, 'A')]
        [TestCase(0, 5, 'X')]
        public void Mask_MaskStartIsN_MasksFromNUntilEndOfString(int start, int length, char mask)
        {
            //arrange
            var source = CreateStringOfLength(length);

            var expected = new string(mask, length - start);

            //act
            var actual = source.Mask(length - start, start, mask).Substring(start, length - start);

            //assert
            actual.Should().Be(expected);
        }

        [TestCase("ABCDEFG", 4, 3, 'x', "ABCxxxx")]
        [TestCase("ZZ", 1, 1, 'Y', "ZY")]
        [TestCase("", 0, 0, 'x', "")]
        [TestCase("123", 2, 1, '0', "100")]
        public void Mask_ReturnsExpectedValue(string source, int count, int start, char mask, string expected)
        {
            source.Mask(count, start, mask).Should().Be(expected);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        public void Mask_StartGreaterThanSourceStringLength_ThrowsArgumentOutOfRangeException(int stringLength)
        {
            //arrange
            var source = CreateStringOfLength(stringLength);
            var start = source.Length + 1;

            //act, assert
            Assert.Throws<ArgumentOutOfRangeException>(() => source.Mask(1, start + 1));
        }

        [TestCase(0, 0, 1)]
        [TestCase(1, 0, 2)]
        [TestCase(5, 2, 5)]
        public void Mask_MaskLengthExtendsPastEndOfString_ThrowsArgumentOutOfRangeException(int stringLength, int start, int count)
        {
            //arrange
            var source = CreateStringOfLength(stringLength);

            Assert.Throws<ArgumentOutOfRangeException>(() => source.Mask(count, start));
        }

        private string CreateStringOfLength(int n)
        {
            var charArr = new char[n];
            for (int i = 0; i < n; i++)
            {
                charArr[i] = _fixture.Create<char>();
            }

            return new string(charArr);
        }
    }
}
