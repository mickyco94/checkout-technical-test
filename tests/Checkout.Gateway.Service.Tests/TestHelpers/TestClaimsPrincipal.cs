using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Checkout.Gateway.Service.Tests.TestHelpers
{
    internal class TestClaimsPrincipal : ClaimsPrincipal
    {
        public TestClaimsPrincipal(IEnumerable<Claim> claims) : base(new ClaimsIdentity(claims))
        {

        }

        public TestClaimsPrincipal(params Claim[] claims) : this(claims.AsEnumerable())
        {

        }
    }
}