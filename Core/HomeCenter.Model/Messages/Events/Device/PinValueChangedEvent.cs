namespace HomeCenter.Model.Messages.Events.Device
{
    public class PinValueChangedEvent : Event
    {
        public static PinValueChangedEvent Create(string deviceUID, int pinNumber, bool isRising)
        {
            return new PinValueChangedEvent()
            {
                MessageSource = deviceUID,
                PinNumber = pinNumber,
                IsRising = isRising
            };
        }

        public int PinNumber
        {
            get => AsInt(MessageProperties.PinNumber);
            set => SetProperty(MessageProperties.PinNumber, value);
        }

        public bool IsRising
        {
            get => AsBool(MessageProperties.IsRising);
            set => SetProperty(MessageProperties.IsRising, value);
        }
    }
}