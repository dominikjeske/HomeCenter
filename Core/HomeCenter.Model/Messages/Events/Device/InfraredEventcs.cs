namespace HomeCenter.Model.Messages.Events.Device
{
    public class InfraredEvent : Event
    {
        public static InfraredEvent Create(string deviceUID, int system, uint commandCode)
        {
            return new InfraredEvent()
            {
                MessageSource = deviceUID,
                System = system,
                CommandCode = commandCode
            };
        }

        public int System
        {
            get => AsInt(MessageProperties.System);
            set => SetProperty(MessageProperties.System, value);
        }

        public uint CommandCode
        {
            get => AsUint(MessageProperties.CommandCode);
            set => SetProperty(MessageProperties.CommandCode, value);
        }
    }
}