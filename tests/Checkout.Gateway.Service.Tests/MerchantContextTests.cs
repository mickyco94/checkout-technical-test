using AutoFixture;
using Checkout.Gateway.Service.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System;
using System.Security.Claims;

namespace Checkout.Gateway.Service.Tests
{
    [TestFixture]
    public class MerchantContextTests
    {
        private MockRepository _mockRepository;
        private IFixture _fixture;

        private Mock<IHttpContextAccessor> _httpContextAccessor;

        private MerchantContext _merchantContext;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _fixture = new Fixture();

            // Mock setup
            _httpContextAccessor = _mockRepository.Create<IHttpContextAccessor>();

            // Mock default
            SetupMockDefaults();

            // Sut instantiation
            _merchantContext = new MerchantContext(
                _httpContextAccessor.Object
            );
        }

        private void SetupMockDefaults()
        {
            _httpContextAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext
            {
                User = new TestClaimsPrincipal()
            });
        }

        [Test]
        public void GetMerchantId_UserDoesNotHaveSidClaim_ThrowsArgumentNullException()
        {
            //arrange

            //act
            Action action = () => _merchantContext.GetMerchantId();

            //assert
            action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(nameof(ClaimTypes.Sid));
        }

        [Test]
        public void GetMerchantId_UserHasSidClaim_ReturnsValue()
        {
            //arrange
            var sidValue = _fixture.Create<string>();
            var sidClaim = new Claim(ClaimTypes.Sid, sidValue);

            _httpContextAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext
            {
                User = new TestClaimsPrincipal(sidClaim)
            });

            //act
            var res = _merchantContext.GetMerchantId();

            //assert
            res.Should().Be(sidValue);
        }
    }
}
