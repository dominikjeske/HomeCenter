using HomeCenter.Abstractions;
using HomeCenter.Abstractions.Defaults;
using HomeCenter.Messages.Commands.Device;
using System.Diagnostics.CodeAnalysis;

namespace HomeCenter.Capabilities
{
    public class PowerState : StateBase
    {
        public static string StateName { get; } = nameof(PowerState);

        public PowerState([AllowNull] string ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.StateName] = nameof(PowerState);
            this[StateProperties.CapabilityName] = Abstractions.Defaults.Capabilities.PowerController;
            this.SetPropertyList(StateProperties.SupportedCommands, nameof(TurnOnCommand), nameof(TurnOffCommand), nameof(SwitchPowerStateCommand));
        }
    }
}