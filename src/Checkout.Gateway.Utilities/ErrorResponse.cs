namespace Checkout.Gateway.Utilities
{
    public class ErrorResponse
    {
        public ErrorResponse(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public string Message { get; }
        public string Code { get; }
    }
}