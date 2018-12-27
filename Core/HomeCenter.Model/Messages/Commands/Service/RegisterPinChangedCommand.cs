using HomeCenter.Model.Messages.Commands;

namespace HomeCenter.Model.Messages.Queries.Service
{
    public class RegisterPinChangedCommand : Command
    {
        public string PinNumber
        {
            get => AsString(MessageProperties.PinNumber);
            set => SetProperty(MessageProperties.PinNumber, value);
        }

        public string PinMode
        {
            get => AsString(MessageProperties.PinMode);
            set => SetProperty(MessageProperties.PinMode, value);
        }
    }
}