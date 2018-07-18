using Wirehome.ComponentModel.Capabilities.Constants;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.ValueTypes;

namespace Wirehome.ComponentModel.Capabilities
{
    public class PlaybackState : State
    {
        public static string StateName { get; } = nameof(PlaybackState);

        public PlaybackState(StringValue ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.Value] = new BooleanValue();
            this[StateProperties.StateName] = new StringValue(nameof(PlaybackState));
            this[StateProperties.CapabilityName] = new StringValue(Constants.Capabilities.PlaybackController);
            this[StateProperties.SupportedCommands] = new StringListValue(CommandType.PlayCommand, CommandType.StopCommand);
        }
    }
}