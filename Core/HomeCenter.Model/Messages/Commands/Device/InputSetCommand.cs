namespace HomeCenter.Model.Messages.Commands.Device
{
    public class InputSetCommand : Command
    {
        public static InputSetCommand Create(string input)
        {
            var command = new InputSetCommand();
            command[MessageProperties.InputSource] = input;
            return command;
        }
    }
}