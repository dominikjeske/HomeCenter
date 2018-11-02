using HomeCenter.Model.ValueTypes;

namespace HomeCenter.Model.Messages.Commands.Device
{
    public class VolumeSetCommand : Command
    {
        public static VolumeSetCommand Create(double volume)
        {
            var command = new VolumeSetCommand();
            command[CommandProperties.Value] = new DoubleValue(volume);
            return command;
        }
    }
}