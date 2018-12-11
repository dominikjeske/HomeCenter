namespace HomeCenter.Model.Messages.Commands.Device
{
    public class VolumeSetCommand : Command
    {
        public static VolumeSetCommand Create(double volume)
        {
            var command = new VolumeSetCommand();
            command.SetProperty(MessageProperties.Value, volume);
            return command;
        }
    }
}