namespace MockBank.API.Client.Models
{
    public class TransferFundsResponse
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public TransferFundsSuccessfulResponse SuccessResponse { get; set; }
        public TransferFundsErrorResponse ErrorResponse { get; set; }
    }
}