using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HomeCenter.TestRunner
{
    internal static class Program
    {
        private static readonly bool autorun = false;

        private static async Task Main(string[] args)
        {
            int programNumber = 1;
            string input = "";

            Console.WriteLine("HomeCenter TestRunner:");
            Console.WriteLine("1. Full controller configuration");
            Console.WriteLine("2. ProtoCluster Playground");
            Console.WriteLine("3. Code generation");

            if (!autorun)
            {
                input = Console.ReadLine();
            }

            if (!int.TryParse(input, out programNumber))
            {
                programNumber = 1;
            }

            switch (programNumber)
            {
                case 1:
                    await StartController().ConfigureAwait(false);
                    break;

                case 2:
                    await StartCluster().ConfigureAwait(false);
                    Console.ReadLine();
                    break;

                case 3:
                    await GenerateDebuging().ConfigureAwait(false);
                    Console.ReadLine();
                    break;
            }
        }

        private static async Task StartController()
        {
            var runners = new Runner[]
            {
                new DenonRunner("DenonComponent"),
                new KodiRunner("KodiComponent"),
                new PcRunner("PcComponent"),
                new SamsungRunner("KodiComponent"),
                new SonyRunner("SonyComponent"),
                new RemoteLampRunner("RemoteLamp3")
            };

            var runner = new WirehomeRunner(runners.ToList());
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