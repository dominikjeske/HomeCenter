using HomeCenter.Model.Capabilities.Constants;

namespace HomeCenter.Model.Capabilities
{
    public class HumidityState : State
    {
        public static string StateName { get; } = nameof(HumidityState);

        public HumidityState(string ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.StateName] = nameof(HumidityState);
            this[StateProperties.CapabilityName] = Constants.Capabilities.TemperatureController;

            //this[StateProperties.SupportedCommands] = new StringListValue(CommandType.TurnOnCommand, CommandType.TurnOffCommand);
        }
    }
}