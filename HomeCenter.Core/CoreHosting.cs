using HomeCenter.Abstractions;
using HomeCenter.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(CoreHosting))]

namespace HomeCenter.Core
{
    public class CoreHosting : IHostingStartup
    {
        public static string Assembly => System.Reflection.Assembly.GetAssembly(typeof(CoreHosting)).GetName().Name;

        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IConcurrencyProvider, ConcurrencyProvider>();
            });
        }
    }
}