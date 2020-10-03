using Microsoft.Extensions.DependencyInjection;
using Polly;
using System;
using System.Net.Http;

namespace MockBank.API.Client
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMockBankApiClient(this IServiceCollection services, Action<HttpClient> configOptions)
        {
            return services
                .AddHttpClient<IMockBankApiClient, MockBankApiClient>(configOptions)
                .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(3, ExponentialBackOff)).Services;
        }

        //TODO: Move to common lib and unit test
        private static TimeSpan ExponentialBackOff(int retryAttempt)
        {
            return TimeSpan.FromMilliseconds(((Math.Pow(2, retryAttempt)) - 1) * 600);
        }
    }
}

