using HomeCenter.ActorCompiler;
using HomeCenter.Configuration;
using HomeCenter.Core;
using HomeCenter.MessageBroker;
using HomeCenter.Quartz;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace HomeCenter.App
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static string GetHostingAssemblies()
        {
            var hostings = $"{CoreHosting.Assembly};{MessageBrokerHosting.Assembly};{ActorCompilerHosting.Assembly};{ConfigurationHosting.Assembly};{QuartzHosting.Assembly}";
#if DEBUG
            hostings += $";{Fakes.LocalFakesHosting.Assembly}";
#endif
            return hostings;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //webBuilder.UseStartup<Startup>();
                    webBuilder.UseSetting(WebHostDefaults.HostingStartupAssembliesKey, GetHostingAssemblies())
                              .UseStartup<Startup>();
                });
    }
}