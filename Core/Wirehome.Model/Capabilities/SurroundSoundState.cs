using Wirehome.ComponentModel.Capabilities.Constants;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.ValueTypes;

namespace Wirehome.ComponentModel.Capabilities
{
    public class SurroundSoundState : State
    {
        public static string StateName { get; } = nameof(SurroundSoundState);

        public SurroundSoundState(StringValue ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.Value] = new StringListValue();
            this[StateProperties.StateName] = new StringValue(nameof(SurroundSoundState));
            this[StateProperties.CapabilityName] = new StringValue(Constants.Capabilities.SpeakerController);
            this[StateProperties.SupportedCommands] = new StringListValue(CommandType.SelectSurroundModeCommand);
        }
    }
}