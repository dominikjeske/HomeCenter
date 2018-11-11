using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Events;

namespace HomeCenter.Model.Capabilities
{
    public class EventSource : BaseObject
    {
        public EventSource(string eventType, string direction)
        {
            SetEmptyProperty(EventProperties.EventTime);
            
            this[EventProperties.EventType] = eventType;
            this[EventProperties.EventDirection] = direction;
        }
    }
}