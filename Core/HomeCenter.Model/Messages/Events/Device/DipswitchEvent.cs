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
            this[MessageProperties.Unit] = code.Unit.ToString();
            this[MessageProperties.System] = code.System.ToString();
            this[MessageProperties.CommandCode] = code.Command.ToString();
            SetProperty(MessageProperties.EventTime, SystemTime.Now);
        }

        public override IEnumerable<string> RoutingAttributes() => new string[] { MessageProperties.Unit, MessageProperties.System };

        public DipswitchCode DipswitchCode => DipswitchCode.ParseCode(AsString(MessageProperties.System), AsString(MessageProperties.Unit), AsString(MessageProperties.CommandCode));
    }
}