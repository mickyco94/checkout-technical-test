namespace Checkout.Gateway.Utilities.Idempotency
{
    public interface IIdempotencyContext
    {
        void InvalidateToken();
        void RollbackInvalidation();
        bool RequestAlreadyProcessed();
    }
}