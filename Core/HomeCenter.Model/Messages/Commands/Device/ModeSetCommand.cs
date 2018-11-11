namespace HomeCenter.Model.Messages.Commands.Device
{
    public class ModeSetCommand : Command
    {
        public static ModeSetCommand Create(string mode)
        {
            var command = new ModeSetCommand();
            command[CommandProperties.SurroundMode] = mode;
            return command;
        }
    }
}