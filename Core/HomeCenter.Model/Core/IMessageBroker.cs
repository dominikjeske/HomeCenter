using HomeCenter.Broker;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Events;
using HomeCenter.Model.Messages.Queries;
using HomeCenter.Model.Messages.Queries.Services;
using Proto;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Model.Core
{
    public interface IMessageBroker
    {
        Task Publish<T>(T message, RoutingFilter routingFilter = null) where T : ActorMessage;

        Task SendToService<T>(T command, RoutingFilter filter = null) where T : Command;

        Task<R> QueryService<T, R>(T query, RoutingFilter filter = null)
            where T : Query;

        Task<R> QueryJsonService<T, R>(T query, RoutingFilter filter = null)
            where T : Query;

        Task<bool> QueryServiceWithVerify<T, Q, R>(T query, R expectedResult, RoutingFilter filter = null)
            where T : Query, IMessageResult<Q, R>
            where Q : class;

        Task<R> Request<T, R>(T message, PID actor) where T : ActorMessage;

        Task<R> Request<T, R>(T message, string uid) where T : ActorMessage;

        void Send(object message, PID destination);

        void Send(object message, string uid, string address = null);

        SubscriptionToken SubscribeForMessage<T>(PID subscriber, RoutingFilter filter = null) where T : ActorMessage;

        SubscriptionToken SubscribeForQuery<T, R>(PID subscriber, RoutingFilter filter = null) where T : Query;

        IObservable<IMessageEnvelope<T>> Observe<T>() where T : Event;

        SubscriptionToken SubscribeForEvent<T>(Action<IMessageEnvelope<T>> action, RoutingFilter filter = null) where T : Event;

        Task PublishWithTranslate(ActorMessage source, ActorMessage destination, RoutingFilter filter = null);

        void SendWithTranslate(ActorMessage source, ActorMessage destination, string address);

        SubscriptionToken SubscribeForEvent<T>(Func<IMessageEnvelope<T>, Task> action, RoutingFilter filter = null) where T : Event;
    }
}