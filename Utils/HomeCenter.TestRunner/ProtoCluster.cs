using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Queries;
using Proto;
using Proto.Router;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.TestRunner
{
    public static class ProtoCluster
    {
        public static async Task Start()
        {
            var context = new RootContext();

            //var props = Props.FromProducer(() => new HttpServiceProxy());

            //var props = Props.FromProducer(() => new DeviceProxy()).WithChildSupervisorStrategy(new OneForOneStrategy(Decider.Decide, 10, null));

            //var pid = context.SpawnNamed(props, "ROOT");

            //Console.WriteLine($"Start {pid.Id}");

            //try
            //{
            //    var tasks = new Task[]
            //    {
            //        Task.Run(() => context.RequestAsync<string>(pid, new HttpQuery())),
            //        Task.Run(() => context.RequestAsync<string>(pid, new HttpQuery())),
            //        Task.Run(() => context.RequestAsync<string>(pid, new HttpQuery()))
            //    };

            //    //var result = await context.RequestAsync<int>(pid, new QueryCapabilities()).ConfigureAwait(false);
            //    await Task.WhenAll(tasks);

            //    Console.WriteLine($"End");
            //}
            //catch (Exception ee)
            //{
            //    Console.WriteLine(ee.ToString());
            //}

            Console.ReadLine();
        }

        internal static class Decider
        {
            public static SupervisorDirective Decide(PID pid, Exception reason)
            {
                switch (reason)
                {
                    //case RecoverableException _:
                    //    return SupervisorDirective.Restart;
                    //case FatalException _:
                    //    return SupervisorDirective.Stop;
                    default:
                        return SupervisorDirective.Escalate;
                }
            }
        }
    }

    public static class ProtoTest
    {
        public static PID Service;

        public static async Task Start()
        {
            var context = new RootContext();
            var props = Props.FromProducer(() => new ClientActor());

            var serviceProps = Router.NewRoundRobinPool(Props.FromProducer(() => new ServiceActor()), 5);

            Service = context.Spawn(serviceProps);

            var jobs = new List<Task>();
            for (int i = 0; i < 3; i++)
            {
                string actorName = $"Actor_{i}";
                jobs.Add(Task.Run(() =>
                {
                    var client = context.SpawnNamed(props, actorName);
                    context.Send(client, new Command());
                }));
            }

            Console.ReadLine();
        }
    }

    public class ClientActor : IActor
    {
        public virtual async Task ReceiveAsync(IContext context)
        {
            if (context.Message is Command)
            {
                Console.WriteLine($"{DateTime.Now.ToLongTimeString()} START processing by {context.Self.Id}");
                var result = await context.RequestAsync<string>(ProtoTest.Service, new Query());
                Console.WriteLine($"{DateTime.Now.ToLongTimeString()} End processing by {context.Self.Id}");
            }

            return;
        }
    }

    public class ServiceActor : IActor
    {
        public async virtual Task ReceiveAsync(IContext context)
        {
            if (context.Message is Query)
            {
                await Task.Delay(5000);

                context.Respond("result");
            }

            return;
        }
    }
}