using HomeCenter.Broker;
using HomeCenter.Model.Core;
using Proto;

namespace HomeCenter.Model.Contracts
{
    public interface IActorFactory
    {
        RootContext Context { get; }

        PID GetExistingActor(string id, string address = default, IContext parent = default);

        PID CreateActor<T>(string id = default, IContext parent = default) where T : class, IActor;

        PID CreateActor<C>(C actorConfig, IContext parent = null) where C : IBaseObject, IPropertySource;
    }
}