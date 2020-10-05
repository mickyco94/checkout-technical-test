using Microsoft.AspNetCore.Mvc;

namespace Checkout.Gateway.API.Filters
{
    public class IdempotencyFilterAttribute : TypeFilterAttribute
    {
        public IdempotencyFilterAttribute() : base(typeof(IdempotencyActionFilter))
        {
        }
    }
}