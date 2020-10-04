using Checkout.Gateway.Utilities.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Checkout.Gateway.Utilities
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUtilities(this IServiceCollection services)
        {
            services.AddSingleton<IDateTime, DateTimeHelper>();
            services.AddSingleton<IGuid, GuidHelper>();
            services.AddSingleton<ICardExpiryValidator, CardExpiryValidator>();

            return services;
        }
    }
}
