using HomeCenter.Abstractions;
using HomeCenter.Abstractions.Defaults;
using HomeCenter.Messages.Commands.Device;
using System.Diagnostics.CodeAnalysis;

namespace HomeCenter.Capabilities
{
    public class SurroundSoundState : StateBase
    {
        public static string StateName { get; } = nameof(SurroundSoundState);

        public SurroundSoundState([AllowNull] string ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.StateName] = nameof(SurroundSoundState);
            this[StateProperties.CapabilityName] = Abstractions.Defaults.Capabilities.SpeakerController;
            this.SetPropertyList(StateProperties.SupportedCommands, nameof(ModeSetCommand));
        }
    }
}