using Checkout.Gateway.Utilities;
using Checkout.Gateway.Utilities.Idempotency;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Checkout.Gateway.API.Filters
{
    public class IdempotencyActionFilter : IActionFilter
    {
        private readonly IIdempotencyContext _idempotencyContext;

        public IdempotencyActionFilter(IIdempotencyContext idempotencyContext)
        {
            _idempotencyContext = idempotencyContext;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.HttpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError)
            {
                _idempotencyContext.RollbackInvalidation();
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (_idempotencyContext.RequestAlreadyProcessed())
            {
                context.Result = new ConflictObjectResult(new ErrorResponse("IDEM_CONFLICT", "Duplicate idempotency key, this request has already been processed"));
            }
            else
            {
                _idempotencyContext.InvalidateToken();
            }
        }
    }
}
