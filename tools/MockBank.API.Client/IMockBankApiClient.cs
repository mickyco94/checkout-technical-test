using MockBank.API.Client.Models;
using System.Threading.Tasks;

namespace MockBank.API.Client
{
    public interface IMockBankApiClient
    {
        Task<TransferFundsResponse> TransferFunds(TransferFundsRequest request);
    }
}