using HomeCenter.Model.Capabilities.Constants;

namespace HomeCenter.Model.Capabilities
{
    public class PowerLevelState : StateBase
    {
        public static string StateName { get; } = nameof(PowerLevelState);

        public PowerLevelState(string ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.StateName] = nameof(PowerLevelState);
            this[StateProperties.CapabilityName] = Constants.Capabilities.PowerLevelController;
            this.SetPropertyList(StateProperties.SupportedCommands, nameof(SetPowerLevelCommand), nameof(AdjustPowerLevelCommand));
        }
    }
}