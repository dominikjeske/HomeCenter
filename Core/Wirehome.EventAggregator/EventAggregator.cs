using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Collections.Generic;
using Wirehome.Core.Extensions;

// 1. Direct message
// 2. Publish to all
// 3. Filter xxx.yyy.zz
// 4. Filter key-value

//TODO: Add dynamic handlers - types that are not registred as singleton to makeinstance of that type and invoke on publish
namespace Wirehome.Core.EventAggregator
{
    public sealed class EventAggregator : IEventAggregator, IDisposable
    {
        private readonly Subscriptions _subscriptions = new Subscriptions();
        public Func<BehaviorChain> DefaultBehavior { get; set; } = () => new BehaviorChain().WithTimeout(TimeSpan.FromMilliseconds(2000));

        public List<BaseCommandHandler> GetSubscriptors<T>(RoutingFilter filter = null)
        {
            return _subscriptions.GetCurrentSubscriptions(typeof(T), filter);
        }

        public async Task<R> QueryAsync<T, R>
        (
           T message,
           RoutingFilter filter = null,
           CancellationToken cancellationToken = default,
           BehaviorChain behaviors = null
        ) where R : class
        {
            var localSubscriptions = GetSubscriptors<T>(filter).OfType<IAsyncCommandHandler>();

            if (!localSubscriptions.Any()) return default;
            if (localSubscriptions.Skip(1).Any()) throw new Exception($"Cannot send [{typeof(T).Name}] message with result to more than two subscriptors");

            var messageEnvelope = new MessageEnvelope<T>(message, cancellationToken, typeof(R));
            var subscriber = localSubscriptions.First();
            var invokeChain = BuildBehaviorChain(behaviors, subscriber);

            return await invokeChain.HandleAsync<T, R>(messageEnvelope).ConfigureAwait(false);
        }

        private IAsyncCommandHandler BuildBehaviorChain(BehaviorChain behaviors, IAsyncCommandHandler subscriber)
        {
            if(behaviors == null) behaviors = DefaultBehavior();
            return behaviors.Build(subscriber);
        }

        public async Task<R> QueryWithResultCheckAsync<T, R>
        (
            T message,
            R expectedResult,
            RoutingFilter filter = null,
            CancellationToken cancellationToken = default,
            BehaviorChain behaviors = null
        ) where R : class
        {
            var result = await QueryAsync<T, R>(message, filter, cancellationToken, behaviors).ConfigureAwait(false);
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
        ) where R : class
        {
            var localSubscriptions = GetSubscriptors<T>(filter).OfType<IAsyncCommandHandler>();

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
        ) where R : class
        {
            var localSubscriptions = GetSubscriptors<T>(filter).OfType<IAsyncCommandHandler>();

            if (!localSubscriptions.Any()) return;

            var messageEnvelope = new MessageEnvelope<T>(message, cancellationToken, typeof(R));

            var publishTask = localSubscriptions.Select(async x =>
            {
                var invokeChain = BuildBehaviorChain(behaviors, x);
                var result = await invokeChain.HandleAsync<T, R>(messageEnvelope).ConfigureAwait(false);
                await Publish(result).ConfigureAwait(false);
            });

            await publishTask.WhenAll(cancellationToken).Unwrap().ConfigureAwait(false);
        }

        public async Task Publish<T>
        (
           T message,
           RoutingFilter filter = null,
           CancellationToken cancellationToken = default,
           BehaviorChain behaviors = null
        )
        {
            var localSubscriptions = GetSubscriptors<T>(filter).OfType<IAsyncCommandHandler>();
            var messageEnvelope = new MessageEnvelope<T>(message, cancellationToken);

            if (!localSubscriptions.Any()) return;

            var result = localSubscriptions.Select(subscription =>
            {
                var invokeChain = BuildBehaviorChain(behaviors, subscription);
                return invokeChain.HandleAsync<T, VoidResult>(messageEnvelope);
            }
            );

            await result.WhenAll(cancellationToken).Unwrap().ConfigureAwait(false);
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

        public IObservable<IMessageEnvelope<T>> Observe<T>()
        {
            return Observable.Create<IMessageEnvelope<T>>(x => Subscribe<T>(x.OnNext));
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