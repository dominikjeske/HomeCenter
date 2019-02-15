namespace HomeCenter.Model.Messages.Commands.Device
{
    public class AdjustPowerLevelCommand : Command
    {
        public static AdjustPowerLevelCommand Create(double delta)
        {
            var command = new AdjustPowerLevelCommand
            {
                Delta = delta
            };
            return command;
        }

        public double Delta
        {
            get => AsDouble(MessageProperties.Delta);
            set => SetProperty(MessageProperties.Delta, value);
        }
    }
}