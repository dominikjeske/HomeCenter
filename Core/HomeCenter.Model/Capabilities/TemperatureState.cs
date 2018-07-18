using HomeCenter.ComponentModel.Capabilities.Constants;
using HomeCenter.ComponentModel.Commands;
using HomeCenter.ComponentModel.ValueTypes;
using HomeCenter.Model.ComponentModel.Capabilities.Constants;

namespace HomeCenter.ComponentModel.Capabilities
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