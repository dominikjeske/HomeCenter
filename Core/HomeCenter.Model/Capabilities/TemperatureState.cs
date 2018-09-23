using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.ValueTypes;
using HomeCenter.Model.ComponentModel.Capabilities.Constants;

namespace HomeCenter.Model.Capabilities
{
    public class TemperatureState : State
    {
        public static string StateName { get; } = nameof(TemperatureState);

        public TemperatureState(StringValue ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.StateName] = new StringValue(nameof(TemperatureState));
            this[StateProperties.CapabilityName] = new StringValue(Constants.Capabilities.TemperatureController);
            this[StateProperties.Value] = new DoubleValue();
            //this[StateProperties.SupportedCommands] = new StringListValue(CommandType.TurnOnCommand, CommandType.TurnOffCommand);
        }
    }
}