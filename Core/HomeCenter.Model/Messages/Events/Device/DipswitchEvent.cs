using System.Collections.Generic;

namespace HomeCenter.Model.Messages.Events.Device
{
    public class DipswitchEvent : Event
    {
        public static DipswitchEvent Create(string mesageSource, string unit, string system, string command)
        {
            return new DipswitchEvent()
            {
                MessageSource = mesageSource,
                Unit = unit,
                System = system,
                CommandCode = command
            };
        }

        public override IEnumerable<string> RoutingAttributes() => new string[]
        {
            MessageProperties.Unit,
            MessageProperties.System
        };

        public string Unit
        {
            get => AsString(MessageProperties.Unit);
            set => SetProperty(MessageProperties.Unit, value);
        }

        public string System
        {
            get => AsString(MessageProperties.System);
            set => SetProperty(MessageProperties.System, value);
        }

        public string CommandCode
        {
            get => AsString(MessageProperties.CommandCode);
            set => SetProperty(MessageProperties.CommandCode, value);
        }
    }
}