using HomeCenter.ComponentModel.Capabilities.Constants;
using HomeCenter.ComponentModel.ValueTypes;
using HomeCenter.Model.Commands.Specialized;

namespace HomeCenter.ComponentModel.Capabilities
{
    public class SurroundSoundState : State
    {
        public static string StateName { get; } = nameof(SurroundSoundState);

        public SurroundSoundState(StringValue ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.Value] = new StringListValue();
            this[StateProperties.StateName] = new StringValue(nameof(SurroundSoundState));
            this[StateProperties.CapabilityName] = new StringValue(Constants.Capabilities.SpeakerController);
            this[StateProperties.SupportedCommands] = new StringListValue(nameof(ModeSetCommand));
        }
    }
}