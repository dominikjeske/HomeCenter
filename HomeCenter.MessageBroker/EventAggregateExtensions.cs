using HomeCenter.EventAggregator;
using System;
using System.Threading;
using System.Threading.Tasks;
using HomeCenter.EventAggregator.Behaviors;

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

        // TODO
        //public static void RegisterHandlers(this IEventAggregator eventAggregator, Container container)
        //{
        //    foreach (var type in container.GetCurrentRegistrations().Where(x => x.ServiceType.GetInterfaces().Any(y => y.IsGenericType && y.GetGenericTypeDefinition() == typeof(IHandler<>))))
        //    {
        //        foreach (var handlerInterface in type.ServiceType.GetInterfaces()
        //                                                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IHandler<>) && x.GenericTypeArguments.Length == 1))
        //        {
        //            var messageType = handlerInterface.GenericTypeArguments.FirstOrDefault();
        //            var methodInfo = handlerInterface.GetMethods().FirstOrDefault();
        //            var delegateType = Expression.GetActionType(methodInfo.GetParameters().Select(p => p.ParameterType).ToArray());
        //            var interfaceMap = type.ServiceType.GetInterfaceMap(handlerInterface);

        //            var messageFilterAttribute = interfaceMap.TargetMethods.FirstOrDefault()?.GetCustomAttributes(false)?.Where(at => at is RoutingFilterAttribute).Cast<RoutingFilterAttribute>().FirstOrDefault();
        //            RoutingFilter messageFilter = null;
        //            if (messageFilterAttribute != null)
        //            {
        //                messageFilter = messageFilterAttribute.ToMessageFilter();
        //            }

        //            if (type.Lifestyle == Lifestyle.Singleton)
        //            {
        //                eventAggregator.Subscribe(messageType, Delegate.CreateDelegate(delegateType, container.GetInstance(type.ServiceType), methodInfo.Name), messageFilter);
        //            }
        //            else
        //            {
        //                eventAggregator.Subscribe(messageType, () => Delegate.CreateDelegate(delegateType, container.GetInstance(type.ServiceType), methodInfo.Name), messageFilter);
        //            }
        //        }
        //    }
        //}
    }
}