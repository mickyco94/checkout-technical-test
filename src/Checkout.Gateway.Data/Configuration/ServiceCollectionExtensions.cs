using Checkout.Gateway.Data.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Checkout.Gateway.Data.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddData(this IServiceCollection services)
        {
            services.AddSingleton<MockDocumentDb>();

            services.AddScoped<IPaymentRecordCreator, PaymentRecordContainer>();
            services.AddScoped<IPaymentRecordReader, PaymentRecordContainer>();
            services.AddScoped<IPaymentRecordUpdater, PaymentRecordContainer>();

            return services;
        }
    }
}
