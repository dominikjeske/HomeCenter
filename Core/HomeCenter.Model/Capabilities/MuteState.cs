using HomeCenter.ComponentModel.Capabilities.Constants;
using HomeCenter.ComponentModel.Commands;
using HomeCenter.ComponentModel.ValueTypes;

namespace HomeCenter.ComponentModel.Capabilities
{
    public class MuteState : State
    {
        public static string StateName { get; } = nameof(MuteState);

        public MuteState(StringValue ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.Value] = new BooleanValue();
            this[StateProperties.StateName] = new StringValue(nameof(MuteState));
            this[StateProperties.CapabilityName] = new StringValue(Constants.Capabilities.SpeakerController);
            this[StateProperties.SupportedCommands] = new StringListValue(CommandType.MuteCommand, CommandType.UnmuteCommand);
        }
    }
}