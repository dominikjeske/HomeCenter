using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using NLog.Web;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace HomeCenter.WindowsService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            bool isService = true;
            if (Debugger.IsAttached || args.Contains("--console"))
            {
                isService = false;
            }

            var pathToContentRoot = Directory.GetCurrentDirectory();
            if (isService)
            {
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                pathToContentRoot = Path.GetDirectoryName(pathToExe);
            }

            var host = new WebHostBuilder()
            .UseKestrel()
            .UseNLog()
            .UseContentRoot(pathToContentRoot)
            .UseIISIntegration()
            .UseStartup<Startup>()
            .UseApplicationInsights()
            .Build();

            if (isService)
            {
                host.RunAsCustomService();
            }
            else
            {
                host.Run();
            }
        }
    }
}