using MockBank.API.Client.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MockBank.API.Client
{
    internal class MockBankApiClient : IMockBankApiClient
    {
        private readonly HttpClient _httpClient;

        private const int HttpStatusCodeUnprocessableEntity = 422;

        public MockBankApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TransferFundsResponse> TransferFunds(TransferFundsRequest request)
        {
            var response = await _httpClient.PostAsync(
                "api/payment",
                new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return HandleFailedResponse((int)response.StatusCode, responseBody);
            }

            var deserialized = JsonConvert.DeserializeObject<TransferFundsSuccessfulResponse>(responseBody);

            return new TransferFundsResponse
            {
                Success = true,
                ErrorResponse = null,
                SuccessResponse = deserialized,
                StatusCode = (int)response.StatusCode
            };
        }

        private static TransferFundsResponse HandleFailedResponse(int statusCode, string body)
        {
            if (statusCode == HttpStatusCodeUnprocessableEntity)
            {
                return new TransferFundsResponse
                {
                    ErrorResponse = JsonConvert.DeserializeObject<TransferFundsErrorResponse>(body),
                    StatusCode = statusCode,
                    Success = false,
                    SuccessResponse = null
                };
            }

            return new TransferFundsResponse
            {
                ErrorResponse = null,
                StatusCode = statusCode,
                Success = false,
                SuccessResponse = null
            };
        }
    }
}