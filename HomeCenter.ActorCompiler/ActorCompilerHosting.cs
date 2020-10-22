using HomeCenter.Abstractions;
using HomeCenter.ActorCompiler;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(ActorCompilerHosting))]

namespace HomeCenter.ActorCompiler
{
    public class ActorCompilerHosting : IHostingStartup
    {
        public static string Assembly => System.Reflection.Assembly.GetAssembly(typeof(ActorCompilerHosting)).GetName().Name;

        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices(s =>
            {
                s.AddSingleton<IRoslynCompilerService, RoslynCompilerService>();
            });
        }
    }
}