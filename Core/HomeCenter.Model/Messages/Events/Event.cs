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

        public bool Equals(Event other) => other != null && Type.Compare(other.Type) == 0 && ToProperiesList().LeftEqual(other.ToProperiesList());

        public virtual IEnumerable<string> RoutingAttributes() => GetPropetiesKeys();

        
        public RoutingFilter GetRoutingFilter(IEnumerable<string> routerAttributes)
        {
            var attributes = routerAttributes?.ToDictionary(k => k, v => this[v].ToString()) ?? new Dictionary<string, string>();
            attributes[EventProperties.EventType] = Type;
            var routingKey = this[MessageProperties.MessageSource].ToString();
            attributes[MessageProperties.MessageSource] = routingKey;

            return new RoutingFilter(routingKey, attributes);
        }
    }
}