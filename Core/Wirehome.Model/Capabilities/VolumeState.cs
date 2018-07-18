using Wirehome.ComponentModel.Capabilities.Constants;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.ValueTypes;

namespace Wirehome.ComponentModel.Capabilities
{
    public class VolumeState : State
    {
        public static string StateName { get; } = nameof(VolumeState);

        public VolumeState(StringValue ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.Value] = new DoubleValue();
            this[StateProperties.MaxValue] = new DoubleValue(100.0);
            this[StateProperties.MinValue] = new DoubleValue(0.0);
            this[StateProperties.StateName] = new StringValue(nameof(VolumeState));
            this[StateProperties.CapabilityName] = new StringValue(Constants.Capabilities.SpeakerController);
            this[StateProperties.SupportedCommands] = new StringListValue(CommandType.VolumeUpCommand, CommandType.VolumeDownCommand, CommandType.VolumeSetCommand);
        }
    }
}