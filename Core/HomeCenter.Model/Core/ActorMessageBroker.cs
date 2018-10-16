using HomeCenter.Broker;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Events;
using HomeCenter.Model.Messages.Queries;
using HomeCenter.Model.Messages.Queries.Services;
using Proto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Model.Core
{
    public class ActorMessageBroker : IActorMessageBroker
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IActorFactory _actorFactory;

        public ActorMessageBroker(IEventAggregator eventAggregator, IActorFactory actorFactory)
        {
            _eventAggregator = eventAggregator;
            _actorFactory = actorFactory;
        }

        public SubscriptionToken SubscribeForMessage<T>(PID subscriber, RoutingFilter filter = null) where T : ActorMessage
        {
            return _eventAggregator.Subscribe<T>(message => _actorFactory.Context.Send(subscriber, message.Message), filter);
        }

        public Task<R> QueryService<T, R>(T query, RoutingFilter filter = null) where T : Query
                                                                                where R : class
        {
            if (query is IFormatableMessage<T> formatableMessage)
            {
                query = formatableMessage.FormatMessage();
            }

            return _eventAggregator.QueryAsync<T, R>(query, filter);
        }

        public async Task<bool> QueryServiceWithVerify<T, Q, R>(T query, R expectedResult, RoutingFilter filter = null) where T : Query, IMessageResult<Q, R>
                                                                                                                        where Q : class
                                                                                                                        where R : class
        {
            var result = await QueryService<T, Q>(query, filter).ConfigureAwait(false);
            return query.Verify(result, expectedResult);
        }

        public Task PublisEvent<T>(T message, IEnumerable<string> routerAttributes = null) where T : Event
        {
            return _eventAggregator.Publish(message, message.GetRoutingFilter(routerAttributes));
        }

        public void Send(ActorMessage message, PID destination)
        {
            _actorFactory.Context.Send(destination, message);
        }

        public void Send(ActorMessage message, string uid)
        {
            var pid = _actorFactory.GetActor(uid);
            _actorFactory.Context.Send(pid, message);
        }

        public Task<R> Request<T, R>(PID actor, T message) where T : ActorMessage => _actorFactory.Context.RequestAsync<R>(actor, message);

        public Task<R> Request<T, R>(string uid, T message) where T : ActorMessage
        {
            var pid = _actorFactory.GetActor(uid);

            return Request<T, R>(pid, message);
        }
    }
}