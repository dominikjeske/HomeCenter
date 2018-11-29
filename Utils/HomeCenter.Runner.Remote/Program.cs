using HomeCenter.Messages;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Remote;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Runner.Remote
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            //string address = "192.168.0.159:8000";
            string address = "127.0.0.1:8000";

            try
            {
                Console.WriteLine("Start client");
                Console.ReadLine();

                var loggerFactory = new LoggerFactory()
                                       .AddConsole();

                Log.SetLoggerFactory(loggerFactory);

                Serialization.RegisterFileDescriptor(ProtoMessagesReflection.Descriptor);

                Proto.Remote.Remote.Start("127.0.0.1", 0);

                var server = new PID(address, "DenonComponent");
                var context = new RootContext();
                context.Send(server, new ProtoCommand() { Type = "VolumeUpCommand" });

                Console.ReadLine();
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.ToString());
            }
        }
    }
}