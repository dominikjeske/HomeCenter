using HomeCenter.Broker;
using HomeCenter.Utils.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace HomeCenter.Model.Messages.Events
{
    public class Event : ActorMessage, System.IEquatable<Event>
    {
        public bool Equals(Event other) => other != null && Type.Compare(other.Type) == 0 && GetProperties().LeftEqual(other.GetProperties()); //TODO check

        public virtual IEnumerable<string> RoutingAttributes() => GetPropetiesKeys();

        public RoutingFilter GetRoutingFilter()
        {
            var attributes = GetProperties().ToDictionary();
            if (!attributes.ContainsKey(EventProperties.EventType))
            {
                attributes.Add(EventProperties.EventType, EventType.PropertyChanged);
            }

            var routingKey = attributes[MessageProperties.MessageSource];
            return new RoutingFilter(routingKey, attributes);
        }
        
        public RoutingFilter GetRoutingFilter(IEnumerable<string> routerAttributes)
        {
            var attributes = routerAttributes?.ToDictionary(k => k, v => this[v]) ?? new Dictionary<string, string>();
            attributes[EventProperties.EventType] = Type;
            var routingKey = this[MessageProperties.MessageSource];
            attributes[MessageProperties.MessageSource] = routingKey;

            return new RoutingFilter(routingKey, attributes);
        }
    }
}