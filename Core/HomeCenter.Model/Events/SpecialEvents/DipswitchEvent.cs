using System;
using HomeCenter.Model.ValueTypes;
using HomeCenter.Core;
using HomeCenter.Core.Hardware.RemoteSockets;
using HomeCenter.Core.Extensions;
using HomeCenter.Model.Extensions;
using System.Collections;
using System.Collections.Generic;
using HomeCenter.Model.Core;

namespace HomeCenter.Model.Events
{
    public class DipswitchEvent : Event
    {
        public DipswitchEvent(string deviceUID, DipswitchCode code)
        {
            Type = EventType.DipswitchCode;
            Uid = Guid.NewGuid().ToString();
            this[EventProperties.SourceDeviceUid] = (StringValue)deviceUID;
            this[EventProperties.Unit] = (StringValue)code.Unit.ToString();
            this[EventProperties.System] = (StringValue)code.System.ToString();
            this[EventProperties.CommandCode] = (StringValue)code.Command.ToString();
            this[EventProperties.EventTime] = (DateTimeValue)SystemTime.Now;
        }

        public override IEnumerable<string> RoutingAttributes() => new string[] { EventProperties.Unit, EventProperties.System };
        
        public DipswitchCode DipswitchCode => DipswitchCode.ParseCode(this[EventProperties.System].AsString(), this[EventProperties.Unit].AsString(), this[EventProperties.CommandCode].AsString());
    }
}
