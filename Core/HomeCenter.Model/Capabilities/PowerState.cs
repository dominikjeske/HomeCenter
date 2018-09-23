using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.ValueTypes;
using HomeCenter.Model.Messages.Commands.Device;

namespace HomeCenter.Model.Capabilities
{
    public class PowerState : State
    {
        public static string StateName { get; } = nameof(PowerState);

        public PowerState(StringValue ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.StateName] = new StringValue(nameof(PowerState));
            this[StateProperties.CapabilityName] = new StringValue(Constants.Capabilities.PowerController);
            this[StateProperties.Value] = new StringValue();
            this[StateProperties.ValueList] = new StringListValue(PowerStateValue.ON, PowerStateValue.OFF);
            this[StateProperties.SupportedCommands] = new StringListValue(nameof(TurnOnCommand), nameof(TurnOffCommand));
        }
    }
}