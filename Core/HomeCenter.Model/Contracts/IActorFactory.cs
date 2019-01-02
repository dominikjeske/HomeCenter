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
    }
}