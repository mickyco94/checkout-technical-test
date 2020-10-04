using AutoFixture;
using Checkout.Gateway.Service.Commands.CreatePayment;
using Checkout.Gateway.Utilities;
using Moq;
using NUnit.Framework;

namespace Checkout.Gateway.Service.Tests.Commands
{
    [TestFixture]
    public class CreatePaymentHandlerTests
    {
        private MockRepository _mockRepository;
        private IFixture _fixture;

        private CreatePaymentHandler _createPaymentHandler;

        private Mock<IGuid> _guid;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _fixture = new Fixture();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));

            _guid = _mockRepository.Create<IGuid>();

            // Mock default
            SetupMockDefaults();

            // Sut instantiation
            _createPaymentHandler = new CreatePaymentHandler(_guid.Object);
        }

        private void SetupMockDefaults()
        {

        }
    }

}
