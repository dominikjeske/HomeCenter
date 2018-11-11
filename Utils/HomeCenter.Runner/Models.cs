using HomeCenter.CodeGeneration;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Events;
using HomeCenter.Model.Messages.Queries.Device;
using Quartz;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    //[ProxyCodeGenerator]
    //public class Device : DeviceActor
    //{
    //    public Device(IScheduler scheduler)
    //    {

    //    }


    //    //protected Task Invoke(Event command)
    //    //{
    //    //    return Task.CompletedTask;
    //    //}

    //    private Task Invoke(TurnOnCommand command)
    //    {
    //        return Task.CompletedTask;
    //    }


    //}

   

    //[CommandBuilder]
    public class CommandBuilder : Command
    {

    }


    public class Command
    {

    }

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