using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.ValueTypes;
using HomeCenter.Model.Commands.Device;

namespace HomeCenter.Model.Capabilities
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