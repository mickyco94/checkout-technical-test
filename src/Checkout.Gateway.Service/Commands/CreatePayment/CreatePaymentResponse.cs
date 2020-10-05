using Checkout.Gateway.Data.Models;

namespace Checkout.Gateway.Service.Commands.CreatePayment
{
    public class CreatePaymentResponse
    {
        public string PaymentId { get; set; }

        public PaymentStatus Status { get; set; }
    }
}