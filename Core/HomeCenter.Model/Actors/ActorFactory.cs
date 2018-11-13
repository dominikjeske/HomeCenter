using HomeCenter.Model.Core;
using HomeCenter.Model.Extensions;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Router;
using System;

namespace HomeCenter.Model.Actors
{
    public class ActorFactory : IActorFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ActorPropsRegistry _actorPropsRegistry;
        private readonly ILogger<ActorFactory> _logger;
        public RootContext Context { get; } = new RootContext();

        public ActorFactory(IServiceProvider serviceProvider, ILogger<ActorFactory> logger, ActorPropsRegistry actorPropsRegistry)
        {
            _serviceProvider = serviceProvider;
            _actorPropsRegistry = actorPropsRegistry;
            _logger = logger;
        }


        public PID GetActor(string id, string address = default, IContext parent = default)
        {
            return GetActor(id, address, parent, () => throw new InvalidOperationException($"Actor not created {id}"));
        }

        public PID GetActor<T>(string id = default, string address = default, IContext parent = default)  where T : class, IActor => GetActor(typeof(T), id, address, parent);

        public PID GetActor(Type actorType, string id = default, string address = default, IContext parent = default)
        {
            id = id ?? actorType.FullName;
            return GetActor(id, address, parent, () => CreateActor(actorType, id, parent, () => new Props().WithProducer(() => _serviceProvider.GetActorProxy(actorType))));
        }

        public PID GetActor(Func<IActor> actorProducer, string id, IContext parent = default, int routing = 0)
        {
            if(routing == 0)
            {
                return GetActor(id, null, parent, () => CreateActor(typeof(object), id, parent, () => new Props().WithProducer(actorProducer)));
            }
            return GetActor(id, null, parent, () => CreateActor(typeof(object), id, parent, () => Router.NewRoundRobinPool(Props.FromProducer(actorProducer), routing)));
        }

        public PID GetRootActor(PID actor)
        {
            var rootSeparator = actor.Id.IndexOf("/");
            if(rootSeparator > -1)
            {
                var rootId = actor.Id.Substring(0, rootSeparator);
                var pid = new PID(actor.Address, rootId);
               return pid;
            }
            return actor;
        }

        public PID GetActor(string uid, string address, IContext parent, Func<PID> create)
        {
            address = address ?? "nonhost";

            var pidId = uid;
            if (parent != null)
            {
                pidId = $"{parent.Self.Id}/{uid}";
            }

            var pid = new PID(address, pidId);
            var reff = ProcessRegistry.Instance.Get(pid);
            if (reff is DeadLetterProcess)
            {
                pid = create();
            }
            return pid;
        }

        private PID CreateActor(Type actorType, string id, IContext parent, Func<Props> producer)
        {
            _logger.LogInformation($"Creating actor {id}");

            if (!_actorPropsRegistry.RegisteredProps.TryGetValue(actorType, out var props))
            {
                props = x => x;
            }

            var props2 = props(producer());
            if (parent == null)
            {
                return Context.SpawnNamed(props2, id);
            }
            return parent.SpawnNamed(props2, id);
        }

        //Router.NewRoundRobinPool(Props.FromProducer(() => new ServiceActor()), 5)
    }
}