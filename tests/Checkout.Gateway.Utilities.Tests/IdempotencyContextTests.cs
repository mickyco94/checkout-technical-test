using AutoFixture;
using Checkout.Gateway.Utilities.Cache;
using Checkout.Gateway.Utilities.Idempotency;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System;

namespace Checkout.Gateway.Utilities.Tests
{
    [TestFixture]
    public class IdempotencyContextTests
    {
        private MockRepository _mockRepository;
        private IFixture _fixture;

        private Mock<IDateTime> _dateTime;
        private Mock<ICache> _cache;
        private Mock<IHttpContextAccessor> _httpContextAccessor;

        private IdempotencyContext _idempotencyContext;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _fixture = new Fixture();

            // Mock setup
            _dateTime = _mockRepository.Create<IDateTime>();
            _cache = _mockRepository.Create<ICache>();
            _httpContextAccessor = _mockRepository.Create<IHttpContextAccessor>();

            // Mock default
            SetupMockDefaults();

            // Sut instantiation
            _idempotencyContext = new IdempotencyContext(
                _dateTime.Object,
                _cache.Object,
                _httpContextAccessor.Object
            );
        }

        private void SetupMockDefaults()
        {
            SetMockRequestIdempotencyKey(_fixture.Create<string>());

            _dateTime
                .Setup(x => x.UtcNow())
                .Returns(_fixture.Create<DateTime>());

            _cache
                .Setup(x => x.Contains(It.IsAny<string>()))
                .Returns(true);

            _cache
                .Setup(x => x.Set(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset?>()));

            _cache
                .Setup(x => x.Delete(It.IsAny<string>()));

            _cache
                .Setup(x => x.Get(It.IsAny<string>()))
                .Returns(_fixture.Create<string>());
        }

        private void SetMockRequestIdempotencyKey(string key)
        {
            var context = new DefaultHttpContext();
            context.Request.Headers[IdempotencyContext.IdempotencyHeaderKey] = key;

            _httpContextAccessor
                .Setup(x => x.HttpContext)
                .Returns(context);
        }

        [TestCase(null)]
        [TestCase("")]
        public void InvalidateToken_HeaderNotSet_DoesNothing(string idempotencyKey)
        {
            //arrange
            SetMockRequestIdempotencyKey(idempotencyKey);

            //act
            _idempotencyContext.InvalidateToken();

            //assert
            _cache.Verify(x => x.Set(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset?>()), Times.Never);
        }

        [Test]
        public void InvalidateToken_HeaderSet_AddsKeyToCacheWithArbitraryValueAnd24HourExpiration()
        {
            //arrange
            var idempotencyKey = _fixture.Create<string>();

            var currentTime = _fixture.Create<DateTime>();

            _dateTime.Setup(x => x.UtcNow()).Returns(currentTime);

            var expected = currentTime.AddDays(1);

            SetMockRequestIdempotencyKey(idempotencyKey);

            //act
            _idempotencyContext.InvalidateToken();

            //assert
            _cache.Verify(x => x.Set(idempotencyKey, It.IsAny<string>(), expected), Times.Once);
        }

        [TestCase(null)]
        [TestCase("")]
        public void RollbackInvalidation_HeaderNotSet_DoesNothing(string idempotencyKey)
        {
            //arrange
            SetMockRequestIdempotencyKey(idempotencyKey);

            //act
            _idempotencyContext.RollbackInvalidation();

            //assert
            _cache.Verify(x => x.Delete(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void RollbackInvalidation_HeaderSet_DeletesCacheEntryUnderKey()
        {
            //arrange
            var idempotencyKey = _fixture.Create<string>();
            SetMockRequestIdempotencyKey(idempotencyKey);

            //act
            _idempotencyContext.RollbackInvalidation();

            //assert
            _cache.Verify(x => x.Delete(idempotencyKey), Times.Once);
        }

        [TestCase(null)]
        [TestCase("")]
        public void RequestAlreadyProcessed_HeaderNotSet_ReturnsFalse(string idempotencyKey)
        {
            //arrange
            SetMockRequestIdempotencyKey(idempotencyKey);

            //act
            var res = _idempotencyContext.RequestAlreadyProcessed();

            //assert
            res.Should().BeFalse();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void RequestAlreadyProcessed_HeaderSet_ReturnsExpectedValue(bool keyPresentInCache)
        {
            //arrange
            var idempotencyKey = _fixture.Create<string>();
            SetMockRequestIdempotencyKey(idempotencyKey);

            _cache.Setup(x => x.Contains(idempotencyKey)).Returns(keyPresentInCache);

            //act
            var res = _idempotencyContext.RequestAlreadyProcessed();

            //assert
            res.Should().Be(keyPresentInCache);
        }
    }
}
