using HomeCenter.Model.Capabilities.Constants;

namespace HomeCenter.Model.Capabilities
{
    public class CurrentState : State
    {
        public static string StateName { get; } = nameof(CurrentState);

        public CurrentState(string? ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.StateName] = nameof(CurrentState);
            this[StateProperties.CapabilityName] = Constants.Capabilities.CurrentController;

            //this[StateProperties.SupportedCommands] = new StringListValue(CommandType.TurnOnCommand, CommandType.TurnOffCommand);
        }
    }
}