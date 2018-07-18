using System;
using Wirehome.ComponentModel.ValueTypes;

namespace Wirehome.ComponentModel.Events
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
