using AutoFixture;
using Checkout.Gateway.Service.Commands.CreatePayment;
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

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _fixture = new Fixture();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));

            // Mock default
            SetupMockDefaults();

            // Sut instantiation
            _createPaymentValidator = new CreatePaymentValidator();
        }

        private void SetupMockDefaults()
        {

        }
    }
}
