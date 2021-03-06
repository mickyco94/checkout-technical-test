﻿using Checkout.Gateway.API.Filters;
using Checkout.Gateway.Service.Commands.CreatePayment;
using Checkout.Gateway.Service.Queries;
using Checkout.Gateway.Utilities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Checkout.Gateway.API.V1.Controllers
{
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PaymentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Processes the specified payment returning the Id corresponding to the payment in the Checkout system if successful
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [IdempotencyFilter]
        [ProducesResponseType(typeof(CreatePaymentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<IActionResult> CreatePaymentRequest(CreatePaymentRequest request)
        {
            var res = await _mediator.Send(request);

            return StatusCode(res.StatusCode, res.Success() ? (object)res.SuccessResponse : res.ErrorResponse);
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> GetPayment(string id)
        {
            var res = await _mediator.Send(new GetPaymentByIdRequest
            {
                Id = id
            });

            return StatusCode(res.StatusCode, res.Success() ? (object)res.SuccessResponse : res.ErrorResponse);
        }
    }
}
