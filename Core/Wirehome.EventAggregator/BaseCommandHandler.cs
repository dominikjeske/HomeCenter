using System;
using System.Reflection;

namespace Wirehome.Core.EventAggregator
{
    public abstract class BaseCommandHandler
    {
        internal Guid Token { get; }
        internal TypeInfo MessageType { get; }
        internal RoutingFilter SubscriptionFilter { get; }
        internal object Handler { get; }

        protected BaseCommandHandler(Type type, Guid token, object handler, RoutingFilter filter)
        {
            MessageType = type.GetTypeInfo();
            Token = token;
            Handler = handler;
            SubscriptionFilter = filter;
        }

        public bool IsFilterMatch(RoutingFilter messageFilter)
        {
            if (messageFilter?.RoutingKey == "*") return true;

            if (SubscriptionFilter == null && messageFilter != null) return false;

            return SubscriptionFilter?.EvaluateFilter(messageFilter) ?? true;
        }
    }
}
