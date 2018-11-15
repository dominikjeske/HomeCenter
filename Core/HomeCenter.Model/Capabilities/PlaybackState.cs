using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Messages.Commands.Device;

namespace HomeCenter.Model.Capabilities
{
    public class PlaybackState : State
    {
        public static string StateName { get; } = nameof(PlaybackState);

        public PlaybackState(string ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.StateName] = nameof(PlaybackState);
            this[StateProperties.CapabilityName] = Constants.Capabilities.PlaybackController;
            SetPropertyList(StateProperties.SupportedCommands, nameof(PlayCommand), nameof(StopCommand));
        }
    }
}