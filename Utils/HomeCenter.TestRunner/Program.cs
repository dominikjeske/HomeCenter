using System;
using System.IO;
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
                    break;

                case 3:
                    await GenerateDebuging().ConfigureAwait(false);
                    break;
            }

            Console.ReadLine();
        }

        private static Task StartController()
        {
            var runner = new WirehomeRunner();
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