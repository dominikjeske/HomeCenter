using HomeCenter.Abstractions;
using HomeCenter.Abstractions.Defaults;
using HomeCenter.Messages.Commands.Device;

namespace HomeCenter.Capabilities
{
    public class MuteState : StateBase
    {
        public static string StateName { get; } = nameof(MuteState);

        public MuteState(string ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.StateName] = nameof(MuteState);
            this[StateProperties.CapabilityName] = Abstractions.Defaults.Capabilities.SpeakerController;
            this.SetPropertyList(StateProperties.SupportedCommands, nameof(MuteCommand), nameof(UnmuteCommand));
        }
    }
}