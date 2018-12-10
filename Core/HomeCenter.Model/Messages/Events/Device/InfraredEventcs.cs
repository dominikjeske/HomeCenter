using System;

namespace HomeCenter.Model.Messages.Events.Device
{
    public class InfraredEvent : Event
    {
        public InfraredEvent(string deviceUID, int system, uint commandCode)
        {
            Type = EventType.InfraredCode;
            Uid = Guid.NewGuid().ToString();
            this[MessageProperties.MessageSource] = deviceUID;
            SetProperty(EventProperties.System, system);
            SetProperty(EventProperties.CommandCode, commandCode);
        }
    }
}