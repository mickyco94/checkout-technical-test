using System.Net;

namespace Checkout.Gateway.Utilities
{
    public class ApiResponse<T>
    {
        public static ApiResponse<T> Success(int statusCode, T successResponse)
        {
            return new ApiResponse<T>
            {
                StatusCode = statusCode,
                ErrorResponse = null,
                SuccessResponse = successResponse
            };
        }

        public static ApiResponse<T> Fail(int statusCode, string code, string message)
        {
            return new ApiResponse<T>
            {
                ErrorResponse = new ErrorResponse(code, message),
                SuccessResponse = default(T),
                StatusCode = statusCode
            };
        }

        public bool Success()
        {
            return StatusCode >= (int)HttpStatusCode.OK && StatusCode <= 299;
        }

        public int StatusCode { get; set; }
        public ErrorResponse ErrorResponse { get; set; }
        public T SuccessResponse { get; set; }
    }
}