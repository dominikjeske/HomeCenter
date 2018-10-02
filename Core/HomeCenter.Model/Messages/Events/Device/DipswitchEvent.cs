using HomeCenter.Model.Codes;
using HomeCenter.Model.Core;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.ValueTypes;
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
            this[MessageProperties.MessageSource] = (StringValue)deviceUID;
            this[EventProperties.Unit] = (StringValue)code.Unit.ToString();
            this[EventProperties.System] = (StringValue)code.System.ToString();
            this[EventProperties.CommandCode] = (StringValue)code.Command.ToString();
            this[EventProperties.EventTime] = (DateTimeValue)SystemTime.Now;
        }

        public override IEnumerable<string> RoutingAttributes() => new string[] { EventProperties.Unit, EventProperties.System };

        public DipswitchCode DipswitchCode => DipswitchCode.ParseCode(this[EventProperties.System].AsString(), this[EventProperties.Unit].AsString(), this[EventProperties.CommandCode].AsString());
    }
}