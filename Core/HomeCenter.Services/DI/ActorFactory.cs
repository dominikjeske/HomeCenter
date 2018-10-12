using HomeCenter.Model.Core;
using HomeCenter.Model.Extensions;
using Microsoft.Extensions.Logging;
using Proto;
using System;

namespace HomeCenter.Services.DI
{
    public class ActorFactory : IActorFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ActorPropsRegistry _actorPropsRegistry;
        private readonly ILogger<ActorFactory> _logger;
        private readonly IActorMessageBroker _actorMessageBroker;

        public ActorFactory(IServiceProvider serviceProvider, ILogger<ActorFactory> logger, ActorPropsRegistry actorPropsRegistry, IActorMessageBroker actorMessageBroker)
        {
            _serviceProvider = serviceProvider;
            _actorPropsRegistry = actorPropsRegistry;
            _logger = logger;
            _actorMessageBroker = actorMessageBroker;
        }

        public PID RegisterActor<T>(T actor, string id = default, string address = default, IContext parent = default)
            where T : IActor
        {
            id = id ?? typeof(T).FullName;
            return GetActor(id, address, parent, () => CreateActor<T>(id, parent, () => new Props().WithProducer(() => actor)));
        }

        public PID GetActor(string id, string address = default, IContext parent = default)
        {
            return GetActor(id, address, parent, () => throw new InvalidOperationException($"Actor not created {id}"));
        }

        public PID GetActor<T>(string id = default, string address = default, IContext parent = default)
            where T : class, IActor => GetActor(typeof(T), id, address, parent);

        public PID GetActor(Type actorType, string id = default, string address = default, IContext parent = default)
        {
            id = id ?? actorType.FullName;
            return GetActor(id, address, parent, () => CreateActor(actorType, id, parent, () => new Props().WithProducer(() => _serviceProvider.GetActorProxy(actorType))));
        }

        public PID GetActor(Func<IActor> actorProducer, string id, IContext parent = default)
        {
            return GetActor(id, null, parent, () => CreateActor(typeof(object), id, parent, () => new Props().WithProducer(actorProducer)));
        }

        public PID GetActor(string id, string address, IContext parent, Func<PID> create)
        {
            address = address ?? "nonhost";

            var pidId = id;
            if (parent != null)
            {
                pidId = $"{parent.Self.Id}/{id}";
            }

            var pid = new PID(address, pidId);
            var reff = ProcessRegistry.Instance.Get(pid);
            if (reff is DeadLetterProcess)
            {
                pid = create();
            }
            return pid;
        }

        private PID CreateActor<T>(string id, IContext parent, Func<Props> producer)
            where T : IActor => CreateActor(typeof(T), id, parent, producer);

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
                return _actorMessageBroker.CreateActor(props2, id);
            }
            return parent.SpawnNamed(props2, id);
        }
    }
}