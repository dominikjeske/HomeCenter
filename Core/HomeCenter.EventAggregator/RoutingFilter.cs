using HomeCenter.Utils.Extensions;
using System.Collections.Generic;

namespace HomeCenter.Broker
{
    public class RoutingFilter
    {
        public string RoutingKey { get; }
        public Dictionary<string, string> RoutingAttributes { get; } = new Dictionary<string, string>();

        public RoutingFilter(string routingKey)
        {
            RoutingKey = routingKey;
        }

        public RoutingFilter(IDictionary<string, string> routingAttributes)
        {
            RoutingAttributes.AddRangeNewOnly(routingAttributes);
        }

        public RoutingFilter(string routingKey, IDictionary<string, string> routingAttributes)
        {
            RoutingKey = routingKey;
            RoutingAttributes.AddRangeNewOnly(routingAttributes);
        }

        public bool EvaluateFilter(object message, RoutingFilter messageFilter)
        {
            // If routing key is defined on subscription we check if it is match with message key
            // If Subscriber or sender use * we skip routing key comparison
            if (!string.IsNullOrWhiteSpace(RoutingKey) && !RoutingKey.InvariantEquals(messageFilter?.RoutingKey) && messageFilter?.RoutingKey != "*" && RoutingKey != "*") return false;

            // If subscription have routing attributes
            if (RoutingAttributes.Count > 0)
            {
                // If message is IPropertiesSource we check properties from message directly
                if (message is IPropertiesSource propertiesSource)
                {
                    foreach (var attribute in RoutingAttributes)
                    {
                        if (!propertiesSource.ContainsProperty(attribute.Key)) return false;

                        if (!propertiesSource[attribute.Key].InvariantEquals(attribute.Value)) return false;
                    }
                }

                // if we have additional routing attributes in message filter
                if (messageFilter?.RoutingAttributes?.Count > 0 && !RoutingAttributes.IsEqual(messageFilter.RoutingAttributes))
                {
                    return false;
                }
            }

            return true;
        }

        public static implicit operator RoutingFilter(string routingKey) => new RoutingFilter(routingKey);
    }
}