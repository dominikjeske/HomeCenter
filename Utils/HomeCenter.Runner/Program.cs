using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    internal static class Program
    {
        public static string ByteArrayToString(byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length * 8);
            foreach (byte b in bytes)
            {
                hex.AppendFormat(Convert.ToString(b, 2).PadLeft(8, '0'));
                hex.Append("-");
            }
            hex.Remove(hex.Length - 1, 1);
                
            return hex.ToString();
        }

        private static async Task Main(string[] args)
        {

            var bytes = new byte[2];
            bytes[1] = 5;
            
            var ss = BitConverter.ToString(bytes);
            var bits = new BitArray(bytes);
            var ccc = bits.ToString();

            var xx = ByteArrayToString(bytes);

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
                Console.WriteLine("1. Full controller configuration");
                Console.WriteLine("2. ProtoCluster Playground");
                Console.WriteLine("3. Code generation");

                input = Console.ReadLine();
                if (!int.TryParse(input, out programNumber))
                {
                    programNumber = 1;
                }
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