using HomeCenter.Broker;
using HomeCenter.Model.Components;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Queries;
using HomeCenter.Services.DI;
using HomeCenter.Services.Quartz;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Router;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.TestRunner
{
    public static class ProtoCluster
    {
        public async static Task Start()
        {
            var context = new RootContext();

            var container = new Container();
            var di = new SimpleInjectorServiceProvider(container);
            var loggerFactory = new LoggerFactory().AddConsole();
            var logger = loggerFactory.CreateLogger<ActorFactory>();
            var lp = loggerFactory.CreateLogger<ComponentProxy>();
            var registry = new ActorPropsRegistry();
            var factory = new ActorFactory(di, logger, new ActorPropsRegistry());

            var jobFactory = new SimpleInjectorJobFactory(container);
            var jobSchedulerFactory = new SimpleInjectorSchedulerFactory(jobFactory);
            var scheduler = await jobSchedulerFactory.GetScheduler().ConfigureAwait(false);
            var ea = new EventAggregator();
            var mb = new ActorMessageBroker(ea, factory);

            var a = factory.GetActor(() => new ComponentProxy(scheduler, mb, lp), "a");
            var b = factory.GetActor(() => new ComponentProxy(scheduler, mb, lp), "b");
            var c = factory.GetActor(() => new ComponentProxy(scheduler, mb, lp), "c");
            var d = factory.GetActor(() => new ComponentProxy(scheduler, mb, lp), "d");

            //var a = factory.GetActor<A>("a");
            //var b = factory.GetActor<A>("b");

            //var props = Props.FromProducer(() => new ClientActor());
            //var props2 = Props.FromProducer(() => new ClientActor());

            //var a = context.SpawnNamed(props, "a");
            //var b = context.SpawnNamed(props, "b");

            factory.Context.Send(a, new TurnOnCommand());
            //context.Send(c, new TurnOnCommand());

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

           // return Task.CompletedTask;
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

    public class A : IActor
    {
        public string Test { get; }

        public virtual async Task ReceiveAsync(IContext context)
        {
            if(context.Message is Started)
            {

            }
            else
            {

            }

            return;
        }
    }

    public static class ProtoTest
    {
        public static PID Service;

        public static Task Start()
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
            return Task.CompletedTask;
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