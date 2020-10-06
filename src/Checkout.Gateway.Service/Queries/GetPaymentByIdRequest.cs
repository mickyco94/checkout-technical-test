using Checkout.Gateway.Utilities;
using MediatR;

namespace Checkout.Gateway.Service.Queries
{
    public class GetPaymentByIdRequest : IRequest<ApiResponse<GetPaymentByIdResponse>>
    {
        public string Id { get; set; }
    }
}