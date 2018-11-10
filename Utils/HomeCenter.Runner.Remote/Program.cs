using HomeCenter.Messages;
using Microsoft.Extensions.Logging;
using Polly;
using Proto;
using Proto.Remote;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Runner.Remote
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                await PollyTest();

                Console.WriteLine("Start client");
                Console.ReadLine();

                var loggerFactory = new LoggerFactory()
                                       .AddConsole();

                Log.SetLoggerFactory(loggerFactory);

                Serialization.RegisterFileDescriptor(TestReflection.Descriptor);

                Proto.Remote.Remote.Start("127.0.0.1", 0);

       
                var server = new PID("127.0.0.1:8000", "chatserver");
                var context = new RootContext();
                context.Send(server, new Messagge() { Message = "Hello" });
                
                Console.ReadLine();
            }
            catch (Exception ee)
            {

                throw;
            }
           
        }

        public static Task PollyTest()
        {
            var policy = Policy.Handle<Exception>().WaitAndRetryAsync(10, _ => TimeSpan.FromSeconds(1), Handler);
            return policy.ExecuteAsync(Go2);

        }

        public static void Go()
        {
            throw new Exception();
        }

        public static Task<int> Go2()
        {
            throw new Exception();
            
        }




        public static void Handler(Exception e, TimeSpan t, int att, Context c)
        {

        }
    }
}
