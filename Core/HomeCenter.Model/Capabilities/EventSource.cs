using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Events;
using HomeCenter.Model.ValueTypes;

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