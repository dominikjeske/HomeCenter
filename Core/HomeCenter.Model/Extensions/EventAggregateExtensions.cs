using CSharpFunctionalExtensions;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Events;
using HomeCenter.Model.ValueTypes;
using HomeCenter.Messaging;
using HomeCenter.Messaging.Behaviors;
using HomeCenter.Model.Core;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using HomeCenter.Model.Messages.Queries;

namespace HomeCenter.Model.Extensions
{
    public static class EventAggregateExtensions
    {
        public static Task<R> QueryAsync<T, R>
        (
            this IEventAggregator eventAggregate,
            T message,
            RoutingFilter filter = null,
            CancellationToken cancellationToken = default,
            TimeSpan? timeout = null,
            int retryCount = 0,
            bool async = false
        ) where R : class
        {
            var chain = new BehaviorChain().WithTimeout(timeout).WithRetry(retryCount).WithAsync(async);
            return eventAggregate.QueryAsync<T, R>(message, filter, cancellationToken, chain);
        }

        public static Task<R> QueryWitTimeoutAsync<T, R>
        (
            this IEventAggregator eventAggregate,
            T message,
            TimeSpan? timeout,
            RoutingFilter filter = null,
            CancellationToken cancellationToken = default

        ) where R : class
        {
            var chain = new BehaviorChain().WithTimeout(timeout);
            return eventAggregate.QueryAsync<T, R>(message, filter, cancellationToken, chain);
        }

        public static void RegisterHandlers(this IEventAggregator eventAggregator, Container container)
        {
            foreach (var type in container.GetCurrentRegistrations().Where(x => x.ServiceType.GetInterfaces().Any(y => y.IsGenericType && y.GetGenericTypeDefinition() == typeof(IHandler<>))))
            {
                foreach (var handlerInterface in type.ServiceType.GetInterfaces()
                                                        .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IHandler<>) && x.GenericTypeArguments.Length == 1))
                {
                    var messageType = handlerInterface.GenericTypeArguments.FirstOrDefault();
                    var methodInfo = handlerInterface.GetMethods().FirstOrDefault();
                    var delegateType = Expression.GetActionType(methodInfo.GetParameters().Select(p => p.ParameterType).ToArray());
                    var interfaceMap = type.ServiceType.GetInterfaceMap(handlerInterface);

                    var messageFilterAttribute = interfaceMap.TargetMethods.FirstOrDefault()?.GetCustomAttributes(false)?.Where(at => at is RoutingFilterAttribute).Cast<RoutingFilterAttribute>().FirstOrDefault();
                    RoutingFilter messageFilter = null;
                    if (messageFilterAttribute != null)
                    {
                        messageFilter = messageFilterAttribute.ToMessageFilter();
                    }

                    if (type.Lifestyle == Lifestyle.Singleton)
                    {
                        eventAggregator.Subscribe(messageType, Delegate.CreateDelegate(delegateType, container.GetInstance(type.ServiceType), methodInfo.Name), messageFilter);
                    }
                    else
                    {
                        eventAggregator.Subscribe(messageType, () => Delegate.CreateDelegate(delegateType, container.GetInstance(type.ServiceType), methodInfo.Name), messageFilter);
                    }
                }
            }
        }

        public static Task<R> QueryDeviceAsync<R>(this IEventAggregator eventAggregator, Query message) where R : BaseObject
        {
            return eventAggregator.QueryAsync<Query, R>(message, new RoutingFilter(message.Uid));
        }

        public static async Task<IValue> QueryForValueType(this IEventAggregator eventAggregator, Query message, string property, IValue defaultValue = null)
        {
            var value = await eventAggregator.QueryAsync<Query, BaseObject>(message).ConfigureAwait(false);
            return value != null ? value[property] : defaultValue ?? NullValue.Value;
        }

        public static Task<R> QueryDeviceAsync<R>(this IEventAggregator eventAggregator, Query message, TimeSpan timeOut) where R : BaseObject
        {
            return eventAggregator.QueryWitTimeoutAsync<Query, R>(message, timeOut, new RoutingFilter(message.Uid));
        }

        public static IDisposable SubscribeForDeviceQuery<T>(this IEventAggregator eventAggregator, Func<IMessageEnvelope<T>, Task<object>> action, string uid) where T : BaseObject
        {
            return eventAggregator.SubscribeForAsyncResult<T>(action, new RoutingFilter(uid));
        }

        public static IDisposable SubscribeForDeviceEvent(this IEventAggregator eventAggregator, Func<IMessageEnvelope<Event>, Task> action, IDictionary<string, string> attributes, string eventType = EventType.PropertyChanged)
        {
            var routingKey = attributes[EventProperties.SourceDeviceUid];
            attributes.Add(EventProperties.EventType, eventType);

            return eventAggregator.SubscribeAsync(action, new RoutingFilter(routingKey, attributes));
        }

        public static IDisposable SubscribeForDeviceCommnd<T>(this IEventAggregator eventAggregator, Func<IMessageEnvelope<T>, Task> action, string deviceUid) where T : Command
        {
            return eventAggregator.SubscribeAsync(action, new RoutingFilter(deviceUid));
        }

        public static Task PublishDeviceEvent<T>(this IEventAggregator eventAggregator, T message) where T : Event
        {
            var routingAttributes = message.RoutingAttributes();
            if (routingAttributes?.Count() > 0)
            {
                return PublishDeviceEvent(eventAggregator, message, routingAttributes);
            }

            return eventAggregator.Publish(message, new RoutingFilter(message[EventProperties.SourceDeviceUid].ToString()));
        }

        public static Task PublishDeviceEvent<T>(this IEventAggregator eventAggregator, T message, IEnumerable<string> routerAttributes) where T : Event
        {
            var routing = routerAttributes.ToDictionary(k => k, v => message[v].ToString());

            routing[EventProperties.SourceDeviceUid] = message[EventProperties.SourceDeviceUid].ToString();
            routing[EventProperties.EventType] = message.Type;

            return eventAggregator.Publish(message, new RoutingFilter(message[EventProperties.SourceDeviceUid].ToString(), routing));
        }

        public static Task PublishDeviceCommnd<T>(this IEventAggregator eventAggregator, T message) where T : Command
        {
            return eventAggregator.Publish(message, new RoutingFilter(message.Uid));
        }
    }
}