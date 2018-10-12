using HomeCenter.Broker;
using HomeCenter.Utils.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace HomeCenter.Model.Messages.Events
{
    public class Event : ActorMessage, System.IEquatable<Event>
    {
        public Event()
        {
            SupressPropertyChangeEvent = true;
        }

        public bool Equals(Event other)
        {
            if (other == null || Type.Compare(other.Type) != 0 || !ToProperiesList().LeftEqual(other.ToProperiesList())) return false;

            return true;
        }

        public virtual IEnumerable<string> RoutingAttributes() => GetPropetiesKeys();

        public RoutingFilter GetRoutingFilter()
        {
            var attributes = GetPropertiesStrings();
            if (!attributes.ContainsKey(EventProperties.EventType))
            {
                attributes.Add(EventProperties.EventType, EventType.PropertyChanged);
            }

            var routingKey = attributes[MessageProperties.MessageSource];
            return new RoutingFilter(routingKey, attributes);
        }

        public RoutingFilter GetRoutingFilter(IEnumerable<string> routerAttributes)
        {
            var attributes = routerAttributes.ToDictionary(k => k, v => this[v].ToString());
            attributes[EventProperties.EventType] = Type;
            var routingKey = this[MessageProperties.MessageSource].ToString();

            return new RoutingFilter(routingKey, attributes);
        }
    }
}