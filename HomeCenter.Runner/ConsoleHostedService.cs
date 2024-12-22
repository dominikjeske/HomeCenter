using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    public sealed class ConsoleHostedService : IHostedService
    {
        private readonly ILogger _logger;

        public ConsoleHostedService(ILogger<ConsoleHostedService> logger)
        {
            _logger = logger;
        }

        async Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            var runner = new HomeCenterRunner();
            await runner.Run();
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}