using System;
using System.Collections.Generic;
using Wirehome.Core.Extensions;

namespace Wirehome.Core.EventAggregator
{
    public class RoutingFilter
    {
        public static readonly RoutingFilter MessageWrite = new RoutingFilter("MessageWrite");
        public static readonly RoutingFilter MessageRead = new RoutingFilter("MessageRead");

        public RoutingFilter(string routingKey)
        {
            RoutingKey = routingKey;
        }

        public RoutingFilter(string routingKey, IDictionary<string, string> routingAttributes)
        {
            RoutingKey = routingKey;
            RoutingAttributes.AddRangeNewOnly(routingAttributes);
        }

        public string RoutingKey { get;}
        public Dictionary<string, string> RoutingAttributes { get; } = new Dictionary<string, string>();

        public bool EvaluateFilter(RoutingFilter messageFilter)
        {
            if (messageFilter == null || RoutingKey.Compare(messageFilter.RoutingKey) != 0) return false;
            if (!RoutingAttributes.IsEqual(messageFilter.RoutingAttributes)) return false;
            return true;
        }

        public static implicit operator RoutingFilter(string routingKey) => new RoutingFilter(routingKey);
    }
}
