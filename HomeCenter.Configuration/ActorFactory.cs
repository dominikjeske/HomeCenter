using HomeCenter.Abstractions;
using HomeCenter.EventAggregator;
using HomeCenter.Model.Extensions;
using Light.GuardClauses;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Router;
using System;
using System.Collections.Generic;

namespace HomeCenter.Model.Actors
{
    public class ActorFactory : IActorFactory
    {
        private const string NONHOST = "nonhost";

        private readonly IServiceProvider _serviceProvider;
        private readonly ActorPropsRegistry _actorPropsRegistry;
        private readonly ILogger<ActorFactory> _logger;
        private readonly IActorLoader _typeLoader;
        private readonly Dictionary<string, PID> _actorsCache = new Dictionary<string, PID>();

        public IRootContext Context { get; }
        public ActorSystem System { get; }

        public ActorFactory(IServiceProvider serviceProvider, ILogger<ActorFactory> logger,
                            IActorLoader typeLoader, ActorPropsRegistry actorPropsRegistry)
        {
            System = new ActorSystem();
            Context = new RootContext(System);

            _serviceProvider = serviceProvider;
            _actorPropsRegistry = actorPropsRegistry;
            _logger = logger;
            _typeLoader = typeLoader;
        }

        public PID GetExistingActor(string id, string? address = default, IContext? parent = default)
        {
            if (_actorsCache.ContainsKey(id)) return _actorsCache[id];

            var pid = CreatePidAddress(id, address, parent);

            var reff = System.ProcessRegistry.Get(pid);
            if (reff is DeadLetterProcess)
            {
                throw new InvalidOperationException($"Actor {pid} is death");
            }
            return pid;
        }

        public PID GetParentActor(PID actor)
        {
            var rootSeparator = actor.Id.LastIndexOf("/");
            if (rootSeparator > -1)
            {
                var rootId = actor.Id.Substring(0, rootSeparator);
                var pid = new PID(actor.Address, rootId);
                return pid;
            }
            return actor;
        }

        public PID CreateActor<C>(C actorConfig, IContext? parent = default) where C : IBaseObject, IPropertySource
        {
            var routing = GetRouting(actorConfig);
            var id = actorConfig.Uid;
            IActor? actorProducer() => _typeLoader.GetProxyType(actorConfig);

            if (routing == 0)
            {
                return GetOrCreateActor(id, null, parent, () => CreateActorInternal(typeof(object), id, parent, () => WithGuardiad(Props.FromProducer(actorProducer), parent)));
            }

            //TODO what when parent is null
            return GetOrCreateActor(id, null, parent, () => CreateActorInternal(typeof(object), id, parent, () => parent?.NewRoundRobinPool(WithChildGuardiad(Props.FromProducer(actorProducer)), routing)));
        }

        public PID CreateActor<T>(string? id = default, IContext? parent = default) where T : class, IActor
        {
            var actorType = typeof(T);
            id ??= actorType.FullName;
            return GetOrCreateActor(id, null, parent, () => CreateActorInternal(actorType, id, parent, () => WithGuardiad(new Props().WithProducer(() => _serviceProvider.GetActorProxy(actorType)), parent)));
        }

        private Props WithGuardiad(Props props, IContext? parent)
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

        private SupervisorDirective Decide(PID pid, Exception reason)
        {
            _logger.LogError(reason, "Exception in device {pid}: {reason}", pid, reason);

            return SupervisorDirective.Resume;
        }

        private PID GetOrCreateActor(string? uid, string? address, IContext? parent, Func<PID> create)
        {
            uid = uid.MustNotBeNullOrWhiteSpace(nameof(uid));

            var pid = CreatePidAddress(uid, address, parent);
            var reff = System.ProcessRegistry.Get(pid);
            if (reff is DeadLetterProcess)
            {
                pid = create();
            }

            if (!_actorsCache.ContainsKey(uid))
            {
                _actorsCache.Add(uid, pid);
            }

            return pid;
        }

        private PID CreatePidAddress(string uid, string? address, IContext? parent)
        {
            address ??= NONHOST;

            var pidId = uid;
            if (parent?.Self != null)
            {
                pidId = $"{parent.Self.Id}/{uid}";
            }

            var pid = new PID(address, pidId);
            return pid;
        }

        private PID CreateActorInternal(Type actorType, string? id, IContext? parent, Func<Props?> producer)
        {
            id = id.MustNotBeNullOrWhiteSpace(nameof(id));

            if (!_actorPropsRegistry.RegisteredProps.TryGetValue(actorType, out var props))
            {
                props = x => x;
            }

            var produced = producer?.Invoke();
            if (produced == null) throw new InvalidOperationException();

            var props2 = props(produced);
            if (parent == null)
            {
                return Context.SpawnNamed(props2, id);
            }
            return parent.SpawnNamed(props2, id);
        }

        private int GetRouting(IPropertySource actorConfig)
        {
            if (actorConfig.ContainsProperty("Routing"))
            {
                //TODO fix this
                var routing = actorConfig["Routing"].ToString()!;
                return int.Parse(routing);
            }

            return 0;
        }
    }
}