using HomeCenter.Model.Core;

namespace HomeCenter.Model.Messages.Commands.Device
{

    public class SetPowerLevelCommand : Command
    {

        public static SetPowerLevelCommand Create(double powerLevel)
        {
            var command = new SetPowerLevelCommand
            {
                PowerLevel = powerLevel
            };
            return command;
        }

        public double PowerLevel
        {
            get => this.AsDouble(MessageProperties.PowerLevel);
            set => this.SetProperty(MessageProperties.PowerLevel, value);
        }
    }
}