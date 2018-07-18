using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Wirehome.Core.EventAggregator
{
    public class Subscriptions
    {
        private readonly List<BaseCommandHandler> _allSubscriptions = new List<BaseCommandHandler>();
        private int _subscriptionRevision;

        private int _localSubscriptionRevision;
        private BaseCommandHandler[] _localSubscriptions;

        internal Guid RegisterAsyncWithResult<T>(Func<IMessageEnvelope<T>, Task> action, RoutingFilter filter)
        {
            var type = typeof(T);
            var key = Guid.NewGuid();
            var subscription = new AsyncWithResultCommandHandler(type, key, action, filter);

            lock (_allSubscriptions)
            {
                _allSubscriptions.Add(subscription);
                _subscriptionRevision++;
            }

            return key;
        }

        internal Guid RegisterAsync<T>(Func<IMessageEnvelope<T>, Task> action, RoutingFilter filter)
        {
            var type = typeof(T);
            var key = Guid.NewGuid();
            var subscription = new AsyncCommandHandler(type, key, action, filter);

            lock (_allSubscriptions)
            {
                _allSubscriptions.Add(subscription);
                _subscriptionRevision++;
            }

            return key;
        }

        internal Guid Register<T>(Action<IMessageEnvelope<T>> action, RoutingFilter filter)
        {
            return RegisterCore(typeof(T), action, filter);
        }

        internal Guid Register(Type messageType, Delegate action, RoutingFilter filter)
        {
            ValidateDelegate(messageType, action);

            return RegisterCore(messageType, action, filter);
        }

        internal Guid Register(Type messageType, Func<Delegate> action, RoutingFilter filter)
        {
            //ValidateDelegate(messageType, action);

            return RegisterCore(messageType, action, filter);
        }

        private static void ValidateDelegate(Type messageType, Delegate action)
        {
            var parameters = action.Method.GetParameters();

            if (action.Method.ReturnType != typeof(void) || parameters.Length != 1 || parameters[0].ParameterType != typeof(IMessageEnvelope<>).MakeGenericType(messageType))
            {
                throw new Exception($"Delegate should be in this format: void Delegate(IMessageEnvelope<{messageType.Name}> param)");
            }
        }

        private Guid RegisterCore(Type messageType, object action, RoutingFilter filter)
        {
            var type = messageType;
            var key = Guid.NewGuid();
            var subscription = new CommandHandler(type, key, action, filter);

            lock (_allSubscriptions)
            {
                _allSubscriptions.Add(subscription);
                _subscriptionRevision++;
            }

            return key;
        }

        public void UnRegister(Guid token)
        {
            lock (_allSubscriptions)
            {
                var subscription = _allSubscriptions.FirstOrDefault(s => s.Token == token);
                var removed = _allSubscriptions.Remove(subscription);

                if (removed) { _subscriptionRevision++; }
            }
        }

        public void Clear()
        {
            lock (_allSubscriptions)
            {
                _allSubscriptions.Clear();
                _subscriptionRevision++;
            }
        }

        public bool IsRegistered(Guid token)
        {
            lock (_allSubscriptions) { return _allSubscriptions.Any(s => s.Token == token); }
        }

        public BaseCommandHandler[] GetCurrentSubscriptions()
        {
            if (_localSubscriptions == null)
            {
                _localSubscriptions = new BaseCommandHandler[0];
            }

            if (_localSubscriptionRevision == _subscriptionRevision)
            {
                return _localSubscriptions;
            }

            BaseCommandHandler[] latestSubscriptions;
            lock (_allSubscriptions)
            {
                latestSubscriptions = _allSubscriptions.ToArray();
                _localSubscriptionRevision = _subscriptionRevision;
            }

            _localSubscriptions = latestSubscriptions;

            return latestSubscriptions;
        }

        public List<BaseCommandHandler> GetCurrentSubscriptions(Type messageType, RoutingFilter filter = null)
        {
            var latestSubscriptions = GetCurrentSubscriptions();
            var msgTypeInfo = messageType.GetTypeInfo();
            var filteredSubscription = new List<BaseCommandHandler>();

            for (var idx = 0; idx < latestSubscriptions.Length; idx++)
            {
                var subscription = latestSubscriptions[idx];

                if (!subscription.MessageType.IsAssignableFrom(msgTypeInfo)) continue;

                if (!subscription.IsFilterMatch(filter)) continue;

                filteredSubscription.Add(subscription);
            }

            return filteredSubscription;
        }
    }
}
