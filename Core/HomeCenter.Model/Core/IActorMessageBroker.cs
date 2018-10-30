using HomeCenter.Broker;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Events;
using HomeCenter.Model.Messages.Queries;
using HomeCenter.Model.Messages.Queries.Services;
using Proto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Model.Core
{
    public interface IActorMessageBroker
    {
        Task PublisEvent<T>(T message, IEnumerable<string> routerAttributes = null) where T : Event;

        Task<R> QueryService<T, R>(T query, RoutingFilter filter = null)
            where T : Query
            where R : class;

        Task<bool> QueryServiceWithVerify<T, Q, R>(T query, R expectedResult, RoutingFilter filter = null)
            where T : Query, IMessageResult<Q, R>
            where Q : class
            where R : class;

        Task<R> Request<T, R>(PID actor, T message) where T : ActorMessage;
        Task<R> Request<T, R>(string uid, T message) where T : ActorMessage;

        void Send(ActorMessage message, PID destination);
        void Send(ActorMessage message, string uid);

        SubscriptionToken SubscribeForCommand<T>(PID subscriber, RoutingFilter filter = null) where T : Command;
        SubscriptionToken SubscribeForQuery<T, R>(PID subscriber, RoutingFilter filter = null) where T : Query;
    }
}