using System;
using System.IO;
using System.Threading.Tasks;

namespace HomeCenter.TestRunner
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            // Runtime
            //await ProtoCluster.Start();

            // await ProtoTest.Start();

            // Debug
            await GenerateDebuging().ConfigureAwait(false);

            Console.ReadLine();
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