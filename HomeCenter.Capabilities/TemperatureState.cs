using HomeCenter.Abstractions;
using HomeCenter.Abstractions.Defaults;

namespace HomeCenter.Capabilities
{
    public class TemperatureState : StateBase
    {
        public static string StateName { get; } = nameof(TemperatureState);

        public TemperatureState(string ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.StateName] = nameof(TemperatureState);
            this[StateProperties.CapabilityName] = Abstractions.Defaults.Capabilities.TemperatureController;
            //this[StateProperties.SupportedCommands] = new StringListValue(CommandType.TurnOnCommand, CommandType.TurnOffCommand);
        }
    }
}