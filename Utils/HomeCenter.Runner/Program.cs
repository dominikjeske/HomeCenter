using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {

            var configuration = GetConfiguration();

            int programNumber = 0;
            string input = "";
            string remote = configuration.GetValue<string>("Address");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine("Wait for system start...");
                programNumber = 1;
            }

            if (programNumber == 0)
            {
                Console.WriteLine("HomeCenter TestRunner:");
                Console.WriteLine("1. Full controller configuration (REMOTE)");
                Console.WriteLine("2. Full controller configuration (LOCAL)");
                Console.WriteLine("3. ProtoCluster Playground");
                Console.WriteLine("4. Code generation");

                input = Console.ReadLine();
                if (!int.TryParse(input, out programNumber))
                {
                    programNumber = 1;
                }
            }

            switch (programNumber)
            {
                case 1:
                    await StartRemoteController().ConfigureAwait(false);
                    break;

                case 2:
                    await StartController().ConfigureAwait(false);
                    break;

                case 3:
                    await StartCluster().ConfigureAwait(false);
                    Console.ReadLine();
                    break;

                case 4:
                    await GenerateDebuging().ConfigureAwait(false);
                    Console.ReadLine();
                    break;
            }
        }

        private static IConfigurationRoot GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();
            return configuration;
        }

        private static Task StartController()
        {
            var runners = new Runner[]
            {
                new DenonRunner("DenonComponent"),
                new KodiRunner("KodiComponent"),
                new PcRunner("PcComponent"),
                new SamsungRunner("KodiComponent"),
                new SonyRunner("SonyComponent"),
                 new RemoteSocketRunner("RemoteLamp3")
            };

            var runner = new WirehomeRunner(runners.ToList());
            return runner.Run();
        }

        private static Task StartRemoteController()
        {
            var runners = new Runner[]
            {
                new DenonRunner("DenonComponent"),
                new KodiRunner("KodiComponent"),
                new PcRunner("PcComponent"),
                new SamsungRunner("KodiComponent"),
                new SonyRunner("SonyComponent"),
                new RemoteSocketRunner("RemoteLamp3"),
                new RaspberryRunner("RaspberryLed")
            };

            var runner = new RemoteWirehomeRunner(runners.ToList());
            return runner.Run();
        }

        private static Task StartCluster()
        {
            return ProtoCluster.Start();
        }

        private static async Task GenerateDebuging()
        {
            try
            {
                var code = await File.ReadAllTextAsync(@"..\..\..\Models.cs").ConfigureAwait(false);

                var result = await new ProxyGeneratorTest().Generate(code).ConfigureAwait(false);

                Console.ReadLine();
                Console.WriteLine(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}