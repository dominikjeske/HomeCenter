using HomeCenter.CodeGeneration;
using HomeCenter.ComponentModel.Components;
using HomeCenter.Model.Commands.Specialized;
using HomeCenter.Model.Queries.Specialized;
using System;
using System.Threading.Tasks;

namespace HomeCenter.TestRunner
{
   
    [ProxyCodeGenerator]
    public class Device : Actor
    {
        protected Task Invoke(TurnOnCommand command)
        {
            return Task.CompletedTask;
        }

        protected Task Invoke(TurnOffCommand command)
        {
            return Task.CompletedTask;
        }

        protected Task<int> Get(CapabilitiesQuery query)
        {
            return Task.FromResult(1);
        }

        protected Task<Result> Get(TagsQuery query)
        {
            return Task.FromResult(new Result());
        }
    }

    [ProxyCodeGenerator]
    public class HttpService : Actor
    {
        private readonly Proto.PID _router;

        protected async Task<string> Handle(TurnOnCommand query)
        {
            Console.WriteLine($"Actor: [{nameof(HttpService)}] | Actor:{query.Context.Self.Id} | Sender:{query.Context.Sender.Id}");

            //query.Context.Forward(_router);

            //return Task.FromResult("");
            var result = await query.Context.RequestAsync<string>(_router, query);

            //query.Context.Forward(handler);
            return result;
        }

        protected override Task OnStarted(Proto.IContext context)
        {
            //var robin = Router.NewRoundRobinPool(Props.FromProducer(() => new HttpServiceHandlerProxy()), 2);

            //_router = context.SpawnNamed(robin, "ROUTER");

            return base.OnStarted(context);
        }
    }

    [ProxyCodeGenerator]
    public class HttpServiceHandler : Actor
    {
        public HttpServiceHandler(string A, int B)
        {

        }

        protected async Task<string> Handle(TurnOnCommand query)
        {
            await Task.Delay(3000);

            Console.WriteLine($"Actor [{nameof(HttpServiceHandler)}] | Actor:{query.Context.Self.Id} | Sender:{query.Context.Sender.Id}");

            return "Test";
        }
    }

    public class Result
    {
    }
}