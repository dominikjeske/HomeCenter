using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.ValueTypes;

namespace HomeCenter.Model.Capabilities
{
    public class MuteState : State
    {
        public static string StateName { get; } = nameof(MuteState);

        public MuteState(StringValue ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.Value] = new BooleanValue();
            this[StateProperties.StateName] = new StringValue(nameof(MuteState));
            this[StateProperties.CapabilityName] = new StringValue(Constants.Capabilities.SpeakerController);
            this[StateProperties.SupportedCommands] = new StringListValue(nameof(MuteCommand), nameof(UnmuteCommand));
        }
    }
}