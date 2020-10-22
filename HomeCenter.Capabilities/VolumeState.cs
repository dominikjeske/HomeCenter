using HomeCenter.Abstractions;
using HomeCenter.Abstractions.Defaults;
using HomeCenter.Messages.Commands.Device;

namespace HomeCenter.Capabilities
{
    public class VolumeState : StateBase
    {
        public static string StateName { get; } = nameof(VolumeState);

        public VolumeState(string ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.MaxValue] = "100.0";
            this[StateProperties.MinValue] = "0.0";
            this[StateProperties.StateName] = nameof(VolumeState);
            this[StateProperties.CapabilityName] = Abstractions.Defaults.Capabilities.SpeakerController;
            this.SetPropertyList(StateProperties.SupportedCommands, nameof(VolumeUpCommand), nameof(VolumeDownCommand), nameof(VolumeSetCommand));
        }
    }
}