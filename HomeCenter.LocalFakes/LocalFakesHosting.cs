using HomeCenter.Abstractions;
using HomeCenter.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(LocalFakesHosting))]

namespace HomeCenter.Fakes
{
    public class LocalFakesHosting : IHostingStartup
    {
        public static string? Assembly => System.Reflection.Assembly.GetAssembly(typeof(LocalFakesHosting))?.GetName()?.Name;

        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices(s =>
            {
                s.AddSingleton<II2cBus, FakeII2cBus>();
                s.AddSingleton<ISerialDevice, FakeISerialDevice>();
                s.AddSingleton<IGpioDevice, FakeGpioDevice>();
            });
        }
    }
}