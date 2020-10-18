using ConcurrentCollections;
using HomeCenter.Broker;
using HomeCenter.Model.Contracts;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Events;
using HomeCenter.Model.Messages.Queries;
using HomeCenter.Model.Messages.Queries.Services;
using HomeCenter.Model.Messages.Scheduler;
using HomeCenter.Model.Quartz;
using Proto;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HomeCenter.Model.Core
{
    public class MessageBroker : IMessageBroker
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IActorFactory _actorFactory;
        private readonly IActorScheduler _actorScheduler;
        private readonly MessageGenerator _messageGenerator = new MessageGenerator();

        private readonly ConcurrentHashSet<SubscriptionCache> _subscriptionCahce = new ConcurrentHashSet<SubscriptionCache>();

        public MessageBroker(IEventAggregator eventAggregator, IActorFactory actorFactory, IActorScheduler actorScheduler)
        {
            _eventAggregator = eventAggregator;
            _actorFactory = actorFactory;
            _actorScheduler = actorScheduler;
        }

        public SubscriptionToken SubscribeForMessage<T>(PID subscriber, bool subscribeOnParent, RoutingFilter filter = null) where T : ActorMessage
        {
            if(subscribeOnParent)
            {
                subscriber = _actorFactory.GetParentActor(subscriber);
            }

            var sub = GetSubscription<T>(subscriber);

            if (!_subscriptionCahce.Add(sub)) return SubscriptionToken.Empty;

            return _eventAggregator.Subscribe<T>(message => _actorFactory.Context.Send(subscriber, message.Message), filter);
        }

        public SubscriptionToken SubscribeForQuery<T, R>(PID subscriber, bool subscribeOnParent, RoutingFilter filter = null) where T : Query
        {
            if (subscribeOnParent)
            {
                subscriber = _actorFactory.GetParentActor(subscriber);
            }

            var sub = GetSubscription<T>(subscriber);

            if (!_subscriptionCahce.Add(sub)) return SubscriptionToken.Empty;

            return _eventAggregator.SubscribeForAsyncResult<T>(async message => await _actorFactory.Context.RequestAsync<R>(subscriber, message.Message), filter);
        }

        private SubscriptionCache GetSubscription<T>(PID subscriber)
        {
            //var rootActor = _actorFactory.GetRootActor(subscriber);
            var sub = new SubscriptionCache(subscriber, typeof(T));
            return sub;
        }

        public SubscriptionToken SubscribeForEvent<T>(Action<IMessageEnvelope<T>> action, RoutingFilter filter = null) where T : Event
        {
            return _eventAggregator.Subscribe(action, filter);
        }

        public Task<R> QueryService<T, R>(T query, RoutingFilter filter = null) where T : Query

        {
            query = FormatMessage(query);

            return _eventAggregator.QueryAsync<T, R>(query, filter);
        }

        public async Task<R> QueryJsonService<T, R>(T query, RoutingFilter filter = null) where T : Query

        {
            query = FormatMessage(query);

            var json = await _eventAggregator.QueryAsync<T, string>(query, filter);
            var result = JsonSerializer.Deserialize<R>(json);
            return result;
        }

        public async Task<bool> QueryServiceWithVerify<T, Q, R>(T query, R expectedResult, RoutingFilter filter = null) where T : Query, IMessageResult<Q, R>
                                                                                                                      where Q : class

        {
            var result = await QueryService<T, Q>(query, filter);
            return query.Verify(result, expectedResult);
        }

        public Task SendToService<T>(T command, RoutingFilter filter = null) where T : Command
        {
            command = FormatMessage(command);

            return _eventAggregator.Publish(command, filter);
        }

        public Task Publish<T>(T message, RoutingFilter routingFilter = null) where T : ActorMessage
        {
            return _eventAggregator.Publish(message, routingFilter);
        }

        public IObservable<IMessageEnvelope<T>> Observe<T>(RoutingFilter routingFilter = null) where T : Event
        {
            return _eventAggregator.Observe<T>(routingFilter);
        }

        public void Send(object message, PID destination)
        {
            _actorFactory.Context.Send(destination, message);
        }

        public void SendWithTranslate(ActorMessage source, ActorMessage destination, string address)
        {
            var command = _messageGenerator.CreateCommand(destination.Type);
            command.SetProperties(source);
            command.SetProperties(destination);

            Send(command, address);
        }

        public Task<Event> PublishWithTranslate(ActorMessage source, ActorMessage destination, RoutingFilter filter = null)
        {
            return _messageGenerator.PublishEvent(source, destination, this, filter);
        }

        public void Send(object message, string uid, string address = null)
        {
            var pid = _actorFactory.GetExistingActor(uid, address);
            _actorFactory.Context.Send(pid, message);
        }

        public Task<R> Request<T, R>(T message, PID actor) where T : ActorMessage => _actorFactory.Context.RequestAsync<R>(actor, message);

        public Task<R> Request<T, R>(T message, string uid) where T : ActorMessage
        {
            var pid = _actorFactory.GetExistingActor(uid);

            return Request<T, R>(message, pid);
        }

        private static T FormatMessage<T>(T query) where T : ActorMessage
        {
            if (query is IFormatableMessage<T> formatableMessage)
            {
                query = formatableMessage.FormatMessage();
            }

            return query;
        }

        public Task SendWithSimpleRepeat(ActorMessageContext message, TimeSpan interval, CancellationToken token = default)
        {
            return _actorScheduler.SendWithSimpleRepeat(message, interval, token);
        }

        public Task SendWithCronRepeat(ActorMessageContext message, string cronExpression, CancellationToken token = default, string calendar = default)
        {
            return _actorScheduler.SendWithCronRepeat(message, cronExpression, token);
        }

        public Task SendAfterDelay(ActorMessageContext message, TimeSpan delay, bool cancelExisting = true, CancellationToken token = default)
        {
            return _actorScheduler.SendAfterDelay(message, delay, cancelExisting, token);
        }

        public Task SendAtTime(ActorMessageContext message, DateTimeOffset time, CancellationToken token = default)
        {
            return _actorScheduler.SendAtTime(message, time, token);
        }

        public Task SendDailyAt(ActorMessageContext message, TimeSpan time, CancellationToken token = default, string calendar = default)
        {
            return _actorScheduler.SendDailyAt(message, time, token, calendar);
        }

        public PID GetPID(string uid, string address = null)
        {
            return _actorFactory.GetExistingActor(uid, address);
        }
    }
}