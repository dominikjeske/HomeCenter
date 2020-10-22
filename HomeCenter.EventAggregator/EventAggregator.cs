using HomeCenter.EventAggregator.Behaviors;
using HomeCenter.EventAggregator.Exceptions;
using HomeCenter.EventAggregator.Handlers;
using HomeCenter.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HomeCenter.EventAggregator
{
    internal sealed class EventAggregator : IEventAggregator, IDisposable
    {
        private const int DEFAULT_TIMEOUT = 20000;

        private readonly Subscriptions _subscriptions = new Subscriptions();
        public Func<BehaviorChain> DefaultBehavior { get; set; } = () => new BehaviorChain().WithTimeout(TimeSpan.FromMilliseconds(DEFAULT_TIMEOUT));

        public List<BaseCommandHandler> GetSubscriptors(object message, RoutingFilter filter = null)
        {
            return _subscriptions.GetCurrentSubscriptions(message, filter);
        }

        public async Task<R> QueryAsync<T, R>
        (
           T message,
           RoutingFilter filter = null,
           CancellationToken cancellationToken = default,
           BehaviorChain behaviors = null
        )
        {
            var localSubscriptions = GetSubscriptors(message, filter).OfType<IAsyncCommandHandler>();

            if (!localSubscriptions.Any()) return default;
            if (localSubscriptions.Skip(1).Any()) throw new QueryException($"Cannot send [{typeof(T).Name}] message with result to more than two subscriptors");

            var messageEnvelope = new MessageEnvelope<T>(message, cancellationToken, typeof(R));
            var subscriber = localSubscriptions.First();
            var invokeChain = BuildBehaviorChain(behaviors, subscriber);

            return await invokeChain.HandleAsync<T, R>(messageEnvelope);
        }

        private IAsyncCommandHandler BuildBehaviorChain(BehaviorChain behaviors, IAsyncCommandHandler subscriber)
        {
            if (behaviors == null) behaviors = DefaultBehavior();
            return behaviors.Build(subscriber);
        }

        public async Task<R> QueryWithResultCheckAsync<T, R>
        (
            T message,
            R expectedResult,
            RoutingFilter filter = null,
            CancellationToken cancellationToken = default,
            BehaviorChain behaviors = null
        )
        {
            var result = await QueryAsync<T, R>(message, filter, cancellationToken, behaviors);
            if (!EqualityComparer<R>.Default.Equals(result, expectedResult))
            {
                throw new WrongResultException(result, expectedResult);
            }
            return result;
        }

        public IObservable<R> QueryWithResults<T, R>
        (
            T message,
            RoutingFilter filter = null,
            CancellationToken cancellationToken = default,
            BehaviorChain behaviors = null
        )
        {
            var localSubscriptions = GetSubscriptors(message, filter).OfType<IAsyncCommandHandler>();

            if (!localSubscriptions.Any()) return Observable.Empty<R>();

            var messageEnvelope = new MessageEnvelope<T>(message, cancellationToken, typeof(R));

            return localSubscriptions.Select
                                      (
                                          x =>
                                          {
                                              var invokeChain = BuildBehaviorChain(behaviors, x);
                                              return invokeChain.HandleAsync<T, R>(messageEnvelope);
                                          }
                                      )
                                     .ToObservable()
                                     .SelectMany(x => x);
        }

        public async Task QueryWithRepublishResult<T, R>
        (
            T message,
            RoutingFilter filter = null,
            CancellationToken cancellationToken = default,
            BehaviorChain behaviors = null
        )
        {
            var localSubscriptions = GetSubscriptors(message, filter).OfType<IAsyncCommandHandler>();

            if (!localSubscriptions.Any()) return;

            var messageEnvelope = new MessageEnvelope<T>(message, cancellationToken, typeof(R));

            var publishTask = localSubscriptions.Select(async x =>
            {
                var invokeChain = BuildBehaviorChain(behaviors, x);
                var result = await invokeChain.HandleAsync<T, R>(messageEnvelope);
                await Publish(result);
            });

            await publishTask.WhenAll(cancellationToken).Unwrap();
        }

        public async Task Publish<T>
        (
           T message,
           RoutingFilter filter = null,
           CancellationToken cancellationToken = default,
           BehaviorChain behaviors = null
        )
        {
            var localSubscriptions = GetSubscriptors(message, filter).OfType<IAsyncCommandHandler>();
            var messageEnvelope = new MessageEnvelope<T>(message, cancellationToken);

            if (!localSubscriptions.Any()) return;

            var result = localSubscriptions.Select(subscription =>
            {
                var invokeChain = BuildBehaviorChain(behaviors, subscription);
                return invokeChain.HandleAsync<T, VoidResult>(messageEnvelope);
            }
            );

            await result.WhenAll(cancellationToken).Unwrap();
        }

        public SubscriptionToken SubscribeForAsyncResult<T>(Func<IMessageEnvelope<T>, Task<object>> action, RoutingFilter filter = null)
        {
            return new SubscriptionToken(_subscriptions.RegisterAsyncWithResult(action, filter), this);
        }

        public SubscriptionToken SubscribeAsync<T>(Func<IMessageEnvelope<T>, Task> action, RoutingFilter filter = null)
        {
            return new SubscriptionToken(_subscriptions.RegisterAsync(action, filter), this);
        }

        public SubscriptionToken Subscribe<T>(Action<IMessageEnvelope<T>> action, RoutingFilter filter = null)
        {
            return new SubscriptionToken(_subscriptions.Register(action, filter), this);
        }

        public SubscriptionToken Subscribe(Type messageType, Delegate action, RoutingFilter filter = null)
        {
            return new SubscriptionToken(_subscriptions.Register(messageType, action, filter), this);
        }

        public SubscriptionToken Subscribe(Type messageType, Func<Delegate> actionFactory, RoutingFilter filter = null)
        {
            return new SubscriptionToken(_subscriptions.Register(messageType, actionFactory, filter), this);
        }

        public IObservable<IMessageEnvelope<T>> Observe<T>(RoutingFilter routingFilter = null)
        {
            return Observable.Create<IMessageEnvelope<T>>(x => Subscribe<T>(x.OnNext, routingFilter));
        }

        public void UnSubscribe(Guid token)
        {
            _subscriptions.UnRegister(token);
        }

        public bool IsSubscribed(Guid token)
        {
            return _subscriptions.IsRegistered(token);
        }

        public void ClearSubscriptions()
        {
            _subscriptions.Clear();
        }

        public void Dispose()
        {
            ClearSubscriptions();
        }
    }
}