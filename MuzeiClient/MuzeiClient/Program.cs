using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MuzeiClient.Interfaces;
using MuzeiClient.Models;
using MuzeiClient.Services;

namespace MuzeiClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .UseSystemd()
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;

                    var options = configuration.GetSection("Configuration").Get<WorkerOptions>();

                    services.AddSingleton(options);
                    services.AddTransient<IMuzeiService, MuzeiService>();
                    services.AddHostedService<Worker>();
                    services.AddLogging(lb =>
                    {
                        lb.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                        lb.AddFile(o => o.RootPath = AppContext.BaseDirectory);
                    });
                });
        }
    }
}