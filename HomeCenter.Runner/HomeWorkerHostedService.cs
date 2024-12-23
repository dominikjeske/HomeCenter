using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    internal sealed class HomeWorkerHostedService(HomeCenter homeCenter, ILogger<HomeWorkerHostedService> logger) : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting HomeWorkerHostedService");

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Stopping HomeWorkerHostedService");

            return base.StopAsync(cancellationToken);
        }
    }
}