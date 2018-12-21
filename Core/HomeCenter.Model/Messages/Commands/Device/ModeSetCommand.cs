namespace HomeCenter.Model.Messages.Commands.Device
{
    public class ModeSetCommand : Command
    {
        public static ModeSetCommand Create(string mode)
        {
            var command = new ModeSetCommand
            {
                SurroundMode = mode
            };
            return command;
        }

        public string SurroundMode
        {
            get => AsString(MessageProperties.SurroundMode);
            set => SetProperty(MessageProperties.SurroundMode, value);
        }
    }
}