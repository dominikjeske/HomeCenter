using HomeCenter.ComponentModel.Capabilities.Constants;
using HomeCenter.ComponentModel.ValueTypes;
using HomeCenter.Model.Commands.Specialized;

namespace HomeCenter.ComponentModel.Capabilities
{
    public class PlaybackState : State
    {
        public static string StateName { get; } = nameof(PlaybackState);

        public PlaybackState(StringValue ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.Value] = new BooleanValue();
            this[StateProperties.StateName] = new StringValue(nameof(PlaybackState));
            this[StateProperties.CapabilityName] = new StringValue(Constants.Capabilities.PlaybackController);
            this[StateProperties.SupportedCommands] = new StringListValue(nameof(PlayCommand), nameof(StopCommand));
        }
    }
}