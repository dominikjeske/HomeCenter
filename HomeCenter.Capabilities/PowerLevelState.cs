using HomeCenter.Abstractions;
using HomeCenter.Abstractions.Defaults;
using HomeCenter.Messages.Commands.Device;
using System.Diagnostics.CodeAnalysis;

namespace HomeCenter.Capabilities
{
    public class PowerLevelState : StateBase
    {
        public static string StateName { get; } = nameof(PowerLevelState);

        public PowerLevelState([AllowNull] string ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.StateName] = nameof(PowerLevelState);
            this[StateProperties.CapabilityName] = Abstractions.Defaults.Capabilities.PowerLevelController;
            this.SetPropertyList(StateProperties.SupportedCommands, nameof(SetPowerLevelCommand), nameof(AdjustPowerLevelCommand));
        }
    }
}