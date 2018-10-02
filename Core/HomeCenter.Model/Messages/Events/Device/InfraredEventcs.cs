using HomeCenter.Model.ValueTypes;
using System;

namespace HomeCenter.Model.Messages.Events.Device
{
    public class InfraredEvent : Event
    {
        public InfraredEvent(string deviceUID, int system, int commandCode)
        {
            Type = EventType.InfraredCode;
            Uid = Guid.NewGuid().ToString();
            this[MessageProperties.MessageSource] = (StringValue)deviceUID;
            this[EventProperties.System] = (IntValue)system;
            this[EventProperties.CommandCode] = (IntValue)commandCode;
        }
    }
}