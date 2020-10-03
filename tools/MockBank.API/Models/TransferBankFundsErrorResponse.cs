namespace MockBank.API.Models
{
    public class TransferBankFundsErrorResponse
    {
        public string Code { get; set; }

        public static TransferBankFundsErrorResponse InsufficientFunds => new TransferBankFundsErrorResponse
        {
            Code = "insufficient_funds"
        };

        public static TransferBankFundsErrorResponse ThreeDSecureRequired => new TransferBankFundsErrorResponse
        {
            Code = "insufficient_funds"
        };
    }
}