using HomeCenter.Abstractions;
using HomeCenter.Abstractions.Defaults;
using System.Diagnostics.CodeAnalysis;

namespace HomeCenter.Capabilities
{
    public class HumidityState : StateBase
    {
        public static string StateName { get; } = nameof(HumidityState);

        public HumidityState([AllowNull] string ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.StateName] = nameof(HumidityState);
            this[StateProperties.CapabilityName] = Abstractions.Defaults.Capabilities.TemperatureController;

            //this[StateProperties.SupportedCommands] = new StringListValue(CommandType.TurnOnCommand, CommandType.TurnOffCommand);
        }
    }
}