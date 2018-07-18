using Wirehome.ComponentModel.Capabilities.Constants;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.ValueTypes;

namespace Wirehome.ComponentModel.Capabilities
{
    public class InputSourceState : State
    {
        public static string StateName { get; } = nameof(InputSourceState);

        public InputSourceState(StringValue ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.Value] = new StringListValue();
            this[StateProperties.StateName] = new StringValue(nameof(InputSourceState));
            this[StateProperties.CapabilityName] = new StringValue(Constants.Capabilities.InputController);
            this[StateProperties.SupportedCommands] = new StringListValue(CommandType.VolumeUpCommand);
        }
    }
}