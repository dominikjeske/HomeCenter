using HomeCenter.Model.Codes;
using HomeCenter.Model.Core;
using System;
using System.Collections.Generic;

namespace HomeCenter.Model.Messages.Events.Device
{
    public class DipswitchEvent : Event
    {
        public DipswitchEvent(string deviceUID, DipswitchCode code)
        {
            Type = EventType.DipswitchCode;
            Uid = Guid.NewGuid().ToString();
            this[MessageProperties.MessageSource] = deviceUID;
            this[EventProperties.Unit] = code.Unit.ToString();
            this[EventProperties.System] = code.System.ToString();
            this[EventProperties.CommandCode] = code.Command.ToString();
            SetProperty(EventProperties.EventTime, SystemTime.Now);
        }

        public override IEnumerable<string> RoutingAttributes() => new string[] { EventProperties.Unit, EventProperties.System };

        public DipswitchCode DipswitchCode => DipswitchCode.ParseCode(AsString(EventProperties.System), AsString(EventProperties.Unit), AsString(EventProperties.CommandCode));
    }
}