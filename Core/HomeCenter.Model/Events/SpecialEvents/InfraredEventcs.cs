using System;
using HomeCenter.Model.ValueTypes;

namespace HomeCenter.Model.Events
{
    public class InfraredEvent : Event
    {
        public InfraredEvent(string deviceUID, int system, int commandCode)
        {
            Type = EventType.InfraredCode;
            Uid = Guid.NewGuid().ToString();
            this[EventProperties.SourceDeviceUid] = (StringValue)deviceUID;
            this[EventProperties.System] = (IntValue)system;
            this[EventProperties.CommandCode] = (IntValue)commandCode;
        }
    }
}
