using Checkout.Gateway.Utilities.Cache;
using Checkout.Gateway.Utilities.Idempotency;
using Checkout.Gateway.Utilities.Regex;
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
            services.AddSingleton<ICardNumberValidator, CardNumberValidator>();
            services.AddSingleton<ISortCodeRegex, SortCodeRegex>();
            services.AddSingleton<IAccountNumberRegex, AccountNumberRegex>();
            services.AddSingleton<ICvvRegex, CvvRegex>();

            services.AddScoped<ICurrencyValidator, CurrencyValidator>();
            services.AddScoped<IIdempotencyContext, IdempotencyContext>();

            services.AddScoped<ICache, Cache.CacheWrapper>();

            return services;
        }
    }
}
