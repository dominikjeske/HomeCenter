namespace HomeCenter.Model.Messages.Commands.Device
{
    public class ModeSetCommand : Command
    {
        public static ModeSetCommand Create(string mode)
        {
            var command = new ModeSetCommand();
            command[MessageProperties.SurroundMode] = mode;
            return command;
        }
    }
}