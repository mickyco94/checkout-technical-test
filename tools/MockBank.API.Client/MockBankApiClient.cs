using MockBank.API.Client.Models;
using Newtonsoft.Json;
using System;
using System.Net;
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
            try
            {
                return await TransferFundsResponseInternal(request);
            }
            catch (Exception e)
            {
                return TransferFundsResponse.ExceptionResponse(e);
            }
        }

        private async Task<TransferFundsResponse> TransferFundsResponseInternal(TransferFundsRequest request)
        {
            var response = await _httpClient.PostAsync(
                "api/payment",
                new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == (HttpStatusCode)HttpStatusCodeUnprocessableEntity)
                {
                    return TransferFundsResponse.Error((int)response.StatusCode, JsonConvert.DeserializeObject<TransferFundsErrorResponse>(responseBody));
                }
                else
                {
                    return TransferFundsResponse.UnknownError((int)response.StatusCode);
                }
            }

            var deserialized = JsonConvert.DeserializeObject<TransferFundsSuccessfulResponse>(responseBody);

            return TransferFundsResponse.Successful(deserialized);
        }
    }
}
