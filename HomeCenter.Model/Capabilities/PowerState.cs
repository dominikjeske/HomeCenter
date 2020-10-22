using HomeCenter.Model.Capabilities.Constants;

namespace HomeCenter.Model.Capabilities
{
    public class PowerState : StateBase
    {
        public static string StateName { get; } = nameof(PowerState);

        public PowerState(string ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.StateName] = nameof(PowerState);
            this[StateProperties.CapabilityName] = Constants.Capabilities.PowerController;
            this.SetPropertyList(StateProperties.SupportedCommands, nameof(TurnOnCommand), nameof(TurnOffCommand), nameof(SwitchPowerStateCommand));
        }
    }
}