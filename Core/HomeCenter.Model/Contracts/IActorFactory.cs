using HomeCenter.Broker;
using HomeCenter.Model.Core;
using Proto;
using System;

namespace HomeCenter.Model.Contracts
{
    public interface IActorFactory
    {
        RootContext Context { get; }

        PID GetExistingActor(string id, string address = default, IContext parent = default);

        PID GetRootActor(PID actor);

        PID CreateActor<T>(string id = default, string address = default, IContext parent = default) where T : class, IActor;

        PID CreateActor(Func<IActor> actorProducer, string id, string address = default, IContext parent = default, int routing = default);
        PID CreatePidAddress(string uid, string address, IContext parent);
        PID CreateActor<C>(C actorConfig, IContext parent = null) where C: IBaseObject, IPropertySource;
    }
}