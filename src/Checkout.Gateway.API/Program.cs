using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Checkout.Gateway.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseSerilog((ctx, cfg) =>
                    {
                        cfg.MinimumLevel.Debug();
                        cfg.WriteTo.Console();
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
