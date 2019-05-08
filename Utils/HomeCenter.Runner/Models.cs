using HomeCenter.CodeGeneration;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Commands.Device;
using Quartz;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Runner.Codegen
{
    //[TestBuilder(typeof(TestBuilderSample), "Prefix_", "")]
    //public partial class MyBuilder
    //{

    //}

    //public class TestBuilderSample
    //{
    //    public string Name { get; set; }

    //    public int Date { get; set; }

    //    public TimeSpan Prefix_Time { get; set; }
    //}




    [ProxyCodeGenerator]
    public class Device : DeviceActor
    {
        public Device(IScheduler scheduler)
        {
        }

        //protected Task Invoke(Event command)
        //{
        //    return Task.CompletedTask;
        //}

        //protected Task Invoke(SonyRegisterQuery command)
        //{
        //    return Task.CompletedTask;
        //}

        [Subscribe(true)]
        protected void Handle(TurnOnCommand command)
        {


        }

        //[Subscribe]
        //protected int Handle2(HttpPostQuery query)
        //{

        //    return 0;

        //}
    }

    //public class SonyRegisterQuery : HttpPostQuery, IFormatableMessage<SonyRegisterQuery>
    //{

    //}

   
    //public class TestEvent : Event
    //{

    //}

    //public class TestCommand : Command
    //{

    //}

   

    //public abstract class Event
    //{

    //}

    //public abstract class Command
    //{
    //}

    //public interface IFormatableMessage<T>
    //{

    //}

    //[CommandBuilder]
    //public class CommandBuilder : Command
    //{
    //}



    //[ProxyCodeGenerator]
    //public class HttpService : Actor
    //{
    //    private readonly Proto.PID _router;

    //    protected async Task<string> Handle(TurnOnCommand query)
    //    {
    //        Console.WriteLine($"Actor: [{nameof(HttpService)}] | Actor:{query.Context.Self.Id} | Sender:{query.Context.Sender.Id}");

    //        //query.Context.Forward(_router);

    //        //return Task.FromResult("");
    //        var result = await query.Context.RequestAsync<string>(_router, query);

    //        //query.Context.Forward(handler);
    //        return result;
    //    }

    //    protected override Task OnStarted(Proto.IContext context)
    //    {
    //        //var robin = Router.NewRoundRobinPool(Props.FromProducer(() => new HttpServiceHandlerProxy()), 2);

    //        //_router = context.SpawnNamed(robin, "ROUTER");

    //        return base.OnStarted(context);
    //    }
    //}

    //[ProxyCodeGenerator]
    //public class HttpServiceHandler : DeviceActor
    //{
    //    public HttpServiceHandler(IEventAggregator eventAggregator) : base(eventAggregator)
    //    {
    //    }

    //    protected async Task<string> Handle(TurnOnCommand query)
    //    {
    //        await Task.Delay(3000);

    //        Console.WriteLine($"Actor [{nameof(HttpServiceHandler)}] | Actor:{query.Context.Self.Id} | Sender:{query.Context.Sender.Id}");

    //        return "Test";
    //    }
    //}

    //public class HttpServiceHandler2 : HttpServiceHandler
    //{
    //    public HttpServiceHandler2(IEventAggregator eventAggregator) : base(eventAggregator)
    //    {
    //    }

    //    protected async Task<string> Handle(TurnOffCommand query)
    //    {
    //        await Task.Delay(3000);

    //        Console.WriteLine($"Actor [{nameof(HttpServiceHandler)}] | Actor:{query.Context.Self.Id} | Sender:{query.Context.Sender.Id}");

    //        return "Test";
    //    }
    //}

    //public class Result
    //{
    //}
}