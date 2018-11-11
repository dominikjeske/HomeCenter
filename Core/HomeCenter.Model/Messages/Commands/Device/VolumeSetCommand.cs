namespace HomeCenter.Model.Messages.Commands.Device
{
    public class VolumeSetCommand : Command
    {
        public static VolumeSetCommand Create(double volume)
        {
            var command = new VolumeSetCommand();
            command.SetProperty(CommandProperties.Value, volume);
            return command;
        }
    }
}