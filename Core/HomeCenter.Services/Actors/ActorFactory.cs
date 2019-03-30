using HomeCenter.Model.Contracts;
using HomeCenter.Model.Extensions;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Router;
using System;

namespace HomeCenter.Model.Actors
{
    public class ActorFactory : IActorFactory
    {
        private const string NONHOST = "nonhost";

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

        public PID GetExistingActor(string id, string address = default, IContext parent = default)
        {
            var pid = CreatePidAddress(id, address, parent);
            var reff = ProcessRegistry.Instance.Get(pid);
            if (reff is DeadLetterProcess)
            {
                throw new InvalidOperationException($"Actor {pid} is death");
            }
            return pid;
        }

        public PID GetRootActor(PID actor)
        {
            var rootSeparator = actor.Id.IndexOf("/");
            if (rootSeparator > -1)
            {
                var rootId = actor.Id.Substring(0, rootSeparator);
                var pid = new PID(actor.Address, rootId);
                return pid;
            }
            return actor;
        }

        public PID CreateActor<T>(string id = default, string address = default, IContext parent = default) where T : class, IActor
        {
            return CreateActorFromType(typeof(T), id, address, parent);
        }

        public PID CreateActor(Func<IActor> actorProducer, string id, string address = default, IContext parent = default, int routing = 0)
        {
            if (routing == 0)
            {
                return GetOrCreateActor(id, address, parent, () => CreateActor(typeof(object), id, parent, () => WithGuardiad(Props.FromProducer(actorProducer), parent)));
            }
            return GetOrCreateActor(id, address, parent, () => CreateActor(typeof(object), id, parent, () => Router.NewRoundRobinPool(WithChildGuardiad(Props.FromProducer(actorProducer)), routing)));
        }

        private PID CreateActorFromType(Type actorType, string id = default, string address = default, IContext parent = default)
        {
            id = id ?? actorType.FullName;
            return GetOrCreateActor(id, address, parent, () => CreateActor(actorType, id, parent, () => WithGuardiad(new Props().WithProducer(() => _serviceProvider.GetActorProxy(actorType)), parent)));
        }


        private Props WithGuardiad(Props props, IContext parent)
        {
            if (parent == null)
            {
                return props.WithGuardianSupervisorStrategy(new OneForOneStrategy(Decide, 3, null));
            }
            else
            {
                return props.WithChildSupervisorStrategy(new OneForOneStrategy(Decide, 3, null));
            }
        }

        private Props WithChildGuardiad(Props props)
        {
            return props.WithChildSupervisorStrategy(new OneForOneStrategy(Decide, 3, null));
        }

        private PID GetOrCreateActor(string uid, string address, IContext parent, Func<PID> create)
        {
            var pid = CreatePidAddress(uid, address, parent);
            var reff = ProcessRegistry.Instance.Get(pid);
            if (reff is DeadLetterProcess)
            {
                pid = create();
            }
            return pid;
        }

        public PID CreatePidAddress(string uid, string address, IContext parent)
        {
            address = address ?? NONHOST;

            var pidId = uid;
            if (parent != null)
            {
                pidId = $"{parent.Self.Id}/{uid}";
            }

            var pid = new PID(address, pidId);
            return pid;
        }

        public SupervisorDirective Decide(PID pid, Exception reason)
        {
            _logger.LogError(reason, $"Exception in device {pid}: {reason}");

            return SupervisorDirective.Resume;
        }

        private PID CreateActor(Type actorType, string id, IContext parent, Func<Props> producer)
        {
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
    }
}