﻿using Checkout.Gateway.Utilities.Encryption;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MockBank.API.Client;
using System;

namespace Checkout.Gateway.Service
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServiceLayer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMockBankApiClient(o => { o.BaseAddress = new Uri(configuration["MockBankApiUrl"]); });

            services.AddMediatR(typeof(ServiceCollectionExtensions));

            services.AddScoped<IMerchantContext, MerchantContext>();

            return services;
        }
    }
}
