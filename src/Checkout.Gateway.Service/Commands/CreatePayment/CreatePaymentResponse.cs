namespace Checkout.Gateway.Service.Commands.CreatePayment
{
    public class CreatePaymentResponse
    {
        public string PaymentId { get; set; }

        public PaymentStatus Status { get; set; }
    }

    public enum PaymentStatus
    {
        Failed,
        Succeeded,
        Rejected
    }
}