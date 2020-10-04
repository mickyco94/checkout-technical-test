using System;

namespace MockBank.API.Client.Models
{
    public class TransferFundsResponse
    {
        public static TransferFundsResponse ExceptionResponse(Exception exception) => new TransferFundsResponse
        {
            Success = false,
            SuccessResponse = null,
            ErrorResponse = null,
            Exception = exception,
            StatusCode = 0
        };

        public static TransferFundsResponse Error(int statusCode, TransferFundsErrorResponse error) => new TransferFundsResponse
        {
            Success = false,
            ErrorResponse = error,
            SuccessResponse = null,
            Exception = null,
            StatusCode = statusCode,
        };

        public static TransferFundsResponse UnknownError(int statusCode) => new TransferFundsResponse
        {
            Success = false,
            ErrorResponse = null,
            SuccessResponse = null,
            Exception = null,
            StatusCode = statusCode,
        };

        public static TransferFundsResponse Successful(TransferFundsSuccessfulResponse successfulResponse) => new TransferFundsResponse
        {
            Success = true,
            SuccessResponse = successfulResponse,
            ErrorResponse = null,
            Exception = null,
            StatusCode = 200
        };

        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public TransferFundsSuccessfulResponse SuccessResponse { get; set; }
        public TransferFundsErrorResponse ErrorResponse { get; set; }
        public Exception Exception { get; set; }
    }
}