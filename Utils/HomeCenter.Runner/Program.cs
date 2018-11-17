using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var configuration = GetConfiguration();

            int programNumber = 1;
            string input = "";
            string remote = configuration.GetValue<string>("Address");

            Console.WriteLine("HomeCenter TestRunner:");
            Console.WriteLine("1. Full controller configuration");
            Console.WriteLine("2. Full controller configuration (REMOTE)");
            Console.WriteLine("3. ProtoCluster Playground");
            Console.WriteLine("4. Code generation");

            if (programNumber == 0)
            {
                input = Console.ReadLine();
                if (!int.TryParse(input, out programNumber))
                {
                    programNumber = 1;
                }
            }

            switch (programNumber)
            {
                case 1:
                    await StartController(null).ConfigureAwait(false);
                    break;

                case 2:
                    await StartController(remote).ConfigureAwait(false);
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

        private static async Task StartController(string address)
        {
            var runners = new Runner[]
            {
                new DenonRunner("DenonComponent", address),
                new KodiRunner("KodiComponent", address),
                new PcRunner("PcComponent", address),
                new SamsungRunner("KodiComponent", address),
                new SonyRunner("SonyComponent", address)
            };

            var runner = new WirehomeRunner(runners.ToList(), address);
            await runner.Initialize().ConfigureAwait(false);
            await runner.Run().ConfigureAwait(false);
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