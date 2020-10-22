using HomeCenter.Model.Capabilities.Constants;

namespace HomeCenter.Model.Capabilities
{
    public class PlaybackState : StateBase
    {
        public static string StateName { get; } = nameof(PlaybackState);

        public PlaybackState(string ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.StateName] = nameof(PlaybackState);
            this[StateProperties.CapabilityName] = Constants.Capabilities.PlaybackController;
            this.SetPropertyList(StateProperties.SupportedCommands, nameof(PlayCommand), nameof(StopCommand));
        }
    }
}