using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MuzeiClient.Interfaces;
using MuzeiClient.Models;

namespace MuzeiClient
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IMuzeiService _muzeiService;
        private readonly WorkerOptions _options;

        public Worker(ILogger<Worker> logger, IMuzeiService muzeiService, WorkerOptions options)
        {
            _options = options;
            _logger = logger;
            _muzeiService = muzeiService;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Muzei service starts");
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Muzei service stops");
            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Muzei process running at: {time}", DateTimeOffset.Now);
                    await _muzeiService.ProcessMuzeiRequest();
                    await Task.Delay(_options.RefreshTime, stoppingToken);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }
        }
    }
}