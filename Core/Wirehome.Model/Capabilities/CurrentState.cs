using Wirehome.ComponentModel.Capabilities.Constants;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Model.ComponentModel.Capabilities.Constants;

namespace Wirehome.ComponentModel.Capabilities
{
    public class CurrentState : State
    {
        public static string StateName { get; } = nameof(CurrentState);

        public CurrentState(StringValue ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.StateName] = new StringValue(nameof(CurrentState));
            this[StateProperties.CapabilityName] = new StringValue(Constants.Capabilities.CurrentController);
            this[StateProperties.Value] = new IntValue();
            //this[StateProperties.SupportedCommands] = new StringListValue(CommandType.TurnOnCommand, CommandType.TurnOffCommand);
        }
    }
}