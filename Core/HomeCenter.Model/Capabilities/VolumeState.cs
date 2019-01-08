using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Messages.Commands.Device;

namespace HomeCenter.Model.Capabilities
{
    public class VolumeState : StateBase
    {
        public static string StateName { get; } = nameof(VolumeState);

        public VolumeState(string ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.MaxValue] = "100.0";
            this[StateProperties.MinValue] = "0.0";
            this[StateProperties.StateName] = nameof(VolumeState);
            this[StateProperties.CapabilityName] = Constants.Capabilities.SpeakerController;
            SetPropertyList(StateProperties.SupportedCommands, nameof(VolumeUpCommand), nameof(VolumeDownCommand), nameof(VolumeSetCommand));
        }
    }
}