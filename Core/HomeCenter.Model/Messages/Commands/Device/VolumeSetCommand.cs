namespace HomeCenter.Model.Messages.Commands.Device
{
    public class VolumeSetCommand : Command
    {
        public static VolumeSetCommand Create(double volume)
        {
            var command = new VolumeSetCommand
            {
                Value = volume
            };
            return command;
        }

        public double Value
        {
            get => AsDouble(MessageProperties.Value);
            set => SetProperty(MessageProperties.Value, value);
        }
    }
}