using Proto;
using System;

namespace HomeCenter.Model.Actors
{
    public interface IActorFactory
    {
        RootContext Context { get; }

        PID GetActor<T>(string id = null, string address = null, IContext parent = null)
            where T : class, IActor;

        PID GetActor(string id, string address = null, IContext parent = null);

        PID GetActor(Type actorType, string id = default, string address = default, IContext parent = default);

        PID GetActor(Func<IActor> actorProducer, string id, IContext parent = default, int routing = 0);
        PID GetRootActor(PID actor);
    }
}