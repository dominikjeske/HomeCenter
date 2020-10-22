using HomeCenter.ActorCompiler;
using HomeCenter.Configuration;
using HomeCenter.Core;
using HomeCenter.MessageBroker;
using HomeCenter.Quartz;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace HomeCenter.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static string GetHostingAssemblies()
        {
            return $"{CoreHosting.Assembly};{MessageBrokerHosting.Assembly};{ActorCompilerHosting.Assembly};{ConfigurationHosting.Assembly};{QuartzHosting.Assembly}";
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseSetting(WebHostDefaults.HostingStartupExcludeAssembliesKey, GetHostingAssemblies())
                              .UseStartup<Startup>();
                });
    }
}