using HomeCenter.Abstractions;
using HomeCenter.Abstractions.Defaults;
using HomeCenter.Messages.Commands.Device;
using System.Diagnostics.CodeAnalysis;

namespace HomeCenter.Capabilities
{
    public class InputSourceState : StateBase
    {
        public static string StateName { get; } = nameof(InputSourceState);

        public InputSourceState([AllowNull] string ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.StateName] = nameof(InputSourceState);
            this[StateProperties.CapabilityName] = Abstractions.Defaults.Capabilities.InputController;
            this.SetPropertyList(StateProperties.SupportedCommands, nameof(InputSetCommand));
        }
    }
}