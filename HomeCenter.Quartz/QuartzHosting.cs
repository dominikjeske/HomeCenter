using HomeCenter.Abstractions;
using HomeCenter.Quartz;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;

[assembly: HostingStartup(typeof(QuartzHosting))]

namespace HomeCenter.Quartz
{
    public class QuartzHosting : IHostingStartup
    {
        public static string? Assembly => System.Reflection.Assembly.GetAssembly(typeof(QuartzHosting))?.GetName()?.Name;

        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices(s =>
            {
                s.AddSingleton<IJobFactory, SimpleInjectorJobFactory>();
                s.AddSingleton<ISchedulerFactory, SimpleInjectorSchedulerFactory>();
                s.AddSingleton<IActorScheduler, ActorScheduler>();
                s.AddSingleton<QuartzInitializer>(); //TODO do init in other way
            });
        }
    }
}