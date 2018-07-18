using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Wirehome.WindowsService
{
    internal class CustomWebHostService : WebHostService
    {
        private ILogger _logger;

        public CustomWebHostService(IWebHost host) : base(host)
        {
            _logger = host.Services.GetRequiredService<ILogger<CustomWebHostService>>();
        }

        protected override void OnStarting(string[] args)
        {
            _logger.LogDebug("OnStarting method called.");
            base.OnStarting(args);
        }

        protected override void OnStarted()
        {
            _logger.LogDebug("OnStarted method called.");
            base.OnStarted();
        }

        protected override void OnStopping()
        {
            _logger.LogDebug("OnStopping method called.");
            base.OnStopping();
        }
    }
}
