using HomeCenter.Messages;
using Proto;
using Proto.Remote;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    public static class ProtoCluster
    {
        public async static Task Start()
        {
            Console.WriteLine("Start listening...");

            var context = new RootContext();
            Serialization.RegisterFileDescriptor(TestReflection.Descriptor);
            Remote.Start("127.0.0.1", 8000);

            var props = Props.FromProducer(() => new A());
            context.SpawnNamed(props, "chatserver");

            Console.ReadLine();
        }
    }

    public class A : IActor
    {
        
        public virtual async Task ReceiveAsync(IContext context)
        {
            if (context.Message is Started)
            {
            }
            else if (context.Message is Messagge)
            {
            }
            else
            {
            }

            return;
        }
    }
}