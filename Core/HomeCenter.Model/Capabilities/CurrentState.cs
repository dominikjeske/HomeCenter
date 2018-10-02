using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.ValueTypes;

namespace HomeCenter.Model.Capabilities
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