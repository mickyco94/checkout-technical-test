using Microsoft.AspNetCore.Mvc;
using MockBank.API.Models;
using System;

namespace MockBank.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private const string ValidCardNumber = "1234 1234 1234 1234";
        private const string InsufficientFundsCardNumber = "0000 0000 0000 0000";
        private const string ThreeDSecureRequiredCardNumber = "3333 3333 3333 3333";

        [HttpPost]
        public IActionResult TransferFunds(TransferFundsBankRequest request)
        {
            return request.Source.CardNumber switch
            {
                ValidCardNumber => Ok(new TransferBankFundsSuccessfulResponse()),
                InsufficientFundsCardNumber => UnprocessableEntity(TransferBankFundsErrorResponse.InsufficientFunds),
                ThreeDSecureRequiredCardNumber => UnprocessableEntity(TransferBankFundsErrorResponse.ThreeDSecureRequired),
                _ => RandomlyGeneratedResult()
            };
        }

        private IActionResult RandomlyGeneratedResult()
        {
            Random rnd = new Random();

            var randomValue = rnd.Next(0, 100);

            if (randomValue >= 10)
            {
                return Ok(new TransferBankFundsSuccessfulResponse());
            }

            if (randomValue < 5)
            {
                return UnprocessableEntity(TransferBankFundsErrorResponse.ThreeDSecureRequired);
            }

            if (randomValue < 1)
            {
                return StatusCode(500);
            }

            return UnprocessableEntity(TransferBankFundsErrorResponse.InsufficientFunds);
        }
    }
}