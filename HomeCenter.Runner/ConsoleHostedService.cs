using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    public sealed class ConsoleHostedService(ILoggerFactory loggerFactory) : IHostedService
    {
        async Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            var runner = new HomeCenterRunner(loggerFactory);
            await runner.Run();
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}