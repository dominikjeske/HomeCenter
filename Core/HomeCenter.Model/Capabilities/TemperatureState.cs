using HomeCenter.Model.Capabilities.Constants;

namespace HomeCenter.Model.Capabilities
{
    public class TemperatureState : StateBase
    {
        public static string StateName { get; } = nameof(TemperatureState);

        public TemperatureState(string ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.StateName] = nameof(TemperatureState);
            this[StateProperties.CapabilityName] = Constants.Capabilities.TemperatureController;
            //this[StateProperties.SupportedCommands] = new StringListValue(CommandType.TurnOnCommand, CommandType.TurnOffCommand);
        }
    }
}