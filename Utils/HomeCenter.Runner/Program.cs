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

        //public static void RegisterSonyClient()
        //{
        //    try
        //    {
        //        var request = new SonyRegisterRequest();
        //        var http = new HttpMessagingService(new EventAggregator());

        //        Console.WriteLine("Sony Bravia TV registration tool.");
        //        Console.WriteLine("");
        //        Console.WriteLine("Write address of Sony Bravia TV (format like: 192.168.0.107)");

        //        var address = Console.ReadLine();
        //        request.Address = address?.Length == 0 ? "192.168.0.107" : address;

        //        await http.SendPostRequest(new MessageEnvelope<SonyRegisterRequest>(request)).ConfigureAwait(false);

        //        Console.WriteLine("Enter PIN from TV:");

        //        request.PIN = Console.ReadLine();

        //        await http.SendPostRequest(new MessageEnvelope<SonyRegisterRequest>(request)).ConfigureAwait(false);

        //        var key = request.ReadAuthKey();

        //        Console.WriteLine("Device was registered successfully. Applicaction hash:");
        //        Console.WriteLine(key);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //    }
        //    finally
        //    {
        //        Console.ReadLine();
        //    }
        //}

    }
}