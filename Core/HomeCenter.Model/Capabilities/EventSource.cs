using HomeCenter.Model.Core;
using HomeCenter.Model.Messages;

namespace HomeCenter.Model.Capabilities
{
    public class EventSource : BaseObject
    {
        public EventSource(string eventType, string direction)
        {
            SetEmptyProperty(MessageProperties.EventTime);

            this[MessageProperties.EventType] = eventType;
            this[MessageProperties.EventDirection] = direction;
        }
    }
}