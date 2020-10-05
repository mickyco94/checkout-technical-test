using Checkout.Gateway.API.Authentication;
using Checkout.Gateway.Data.Configuration;
using Checkout.Gateway.Service;
using Checkout.Gateway.Service.Commands.CreatePayment;
using Checkout.Gateway.Utilities;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Collections.Generic;

namespace Checkout.Gateway.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddServiceLayer(Configuration);

            services.AddUtilities();

            services.AddData();

            services.AddMemoryCache();

            services.AddHttpContextAccessor();

            services.AddSwaggerGen();

            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            services.AddAuthentication(AuthenticationConstants.ApiKeyAuthenticationScheme)
                .AddApiKeyAuthentication(o => o.MerchantKeys = new Dictionary<string, string>
                {
                    {"test_key", "amazon"}
                });

            services.AddAuthorization();

            services.AddValidatorsFromAssemblyContaining(typeof(CreatePaymentValidator));

            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
            });

            services.AddControllers().AddFluentValidation(o => o.RegisterValidatorsFromAssemblyContaining<CreatePaymentValidator>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
        IApplicationBuilder app,
        IWebHostEnvironment env,
        IApiVersionDescriptionProvider apiVersionDescriptionProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseSerilogRequestLogging();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
