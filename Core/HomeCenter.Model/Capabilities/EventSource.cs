using HomeCenter.Model.Events;
using HomeCenter.Model.ValueTypes;
using HomeCenter.Model.Core;

namespace HomeCenter.Model.Capabilities
{
    public class EventSource : BaseObject
    {
        public EventSource(string eventType, string direction)
        {
            this[EventProperties.EventTime] = new DateTimeValue();
            this[EventProperties.EventType] = new StringValue(eventType);
            this[EventProperties.EventDirection] = new StringValue(direction);
        }
    }
}