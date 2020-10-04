using Checkout.Gateway.API.V1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Checkout.Gateway.API.V1.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class PaymentController : ControllerBase
    {
        /// <summary>
        /// Processes the specified payment returning the Id corresponding to the payment in the Checkout system if successful
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(CreatePaymentResponse), StatusCodes.Status200OK)]
        [HttpPost]
        public async Task<IActionResult> CreatePaymentRequest(CreatePaymentRequest request)
        {
            return Ok(new CreatePaymentResponse
            {
                PaymentId = Guid.NewGuid().ToString()
            });
        }
    }
}
