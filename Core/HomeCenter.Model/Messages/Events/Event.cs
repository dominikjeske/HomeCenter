using HomeCenter.Broker;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HomeCenter.Model.Messages.Events
{
    public class Event : ActorMessage, IEquatable<Event>
    {
        public Event()
        {
            Uid = Guid.NewGuid().ToString();
            EventTime = SystemTime.Now;
        }

        public bool Equals(Event other) => other != null && Type.Compare(other.Type) == 0 && GetProperties().LeftEqual(other.GetProperties()); //TODO check

        public virtual IEnumerable<string> RoutingAttributes() => GetPropetiesKeys();

        public RoutingFilter GetRoutingFilter()
        {
            var attributes = GetProperties().ToDictionary();
            if (!attributes.ContainsKey(MessageProperties.EventType))
            {
                attributes.Add(MessageProperties.EventType, nameof(PropertyChangedEvent)); //TODO  check
            }

            var routingKey = attributes[MessageProperties.MessageSource];
            return new RoutingFilter(routingKey, attributes);
        }
        
        public RoutingFilter GetRoutingFilter(IEnumerable<string> routerAttributes)
        {
            if (!ContainsProperty(MessageProperties.MessageSource) && routerAttributes == null) return null;

            var attributes = routerAttributes?.ToDictionary(k => k, v => this[v]) ?? new Dictionary<string, string>();
            attributes[MessageProperties.EventType] = Type;
            var routingKey = this[MessageProperties.MessageSource];
            attributes[MessageProperties.MessageSource] = routingKey;

            return new RoutingFilter(routingKey, attributes);
        }

        public DateTimeOffset EventTime
        {
            get => AsDate(MessageProperties.EventTime);
            set => SetProperty(MessageProperties.EventTime, value);
        }

        public string MessageSource
        {
            get => AsString(MessageProperties.MessageSource);
            set => SetProperty(MessageProperties.MessageSource, value);
        }
    }
}