using System;

namespace MockBank.API.Models
{
    public class TransferBankFundsSuccessfulResponse
    {
        public TransferBankFundsSuccessfulResponse()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
    }
}