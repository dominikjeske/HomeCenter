namespace HomeCenter.Model.Messages.Commands.Device
{
    public class InputSetCommand : Command
    {
        public static InputSetCommand Create(string input)
        {
            var command = new InputSetCommand
            {
                InputSource = input
            };
            return command;
        }

        public string InputSource
        {
            get => AsString(MessageProperties.InputSource);
            set => SetProperty(MessageProperties.InputSource, value);
        }
    }
}