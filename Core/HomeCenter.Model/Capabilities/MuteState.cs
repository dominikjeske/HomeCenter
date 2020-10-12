using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Messages.Commands.Device;

namespace HomeCenter.Model.Capabilities
{
    public class MuteState : StateBase
    {
        public static string StateName { get; } = nameof(MuteState);

        public MuteState(string ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.StateName] = nameof(MuteState);
            this[StateProperties.CapabilityName] = Constants.Capabilities.SpeakerController;
            this.SetPropertyList(StateProperties.SupportedCommands, nameof(MuteCommand), nameof(UnmuteCommand));
        }
    }
}