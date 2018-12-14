using HomeCenter.Model.Core;
using System;
using System.Collections.Generic;

namespace HomeCenter.Model.Messages.Events.Device
{
    public class DipswitchEvent : Event
    {
        public DipswitchEvent(string deviceUID, string unit, string system, string command)
        {
            Type = EventType.DipswitchCode;
            Uid = Guid.NewGuid().ToString();
            this[MessageProperties.MessageSource] = deviceUID;
            this[MessageProperties.Unit] = unit;
            this[MessageProperties.System] = system;
            this[MessageProperties.CommandCode] = command;
            SetProperty(MessageProperties.EventTime, SystemTime.Now);
        }

        public override IEnumerable<string> RoutingAttributes() => new string[] { MessageProperties.Unit, MessageProperties.System };
    }
}