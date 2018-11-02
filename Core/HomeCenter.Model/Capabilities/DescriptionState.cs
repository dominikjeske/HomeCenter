using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Model.ValueTypes;

namespace HomeCenter.Model.Capabilities
{

    public class DescriptionState : State
    {
        public static string StateName { get; } = nameof(DescriptionState);

        public DescriptionState(StringValue ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.Value] = new StringValue();
            this[StateProperties.StateName] = new StringValue(nameof(DescriptionState));
            this[StateProperties.CapabilityName] = new StringValue(Constants.Capabilities.InfoController);
            this[StateProperties.SupportedCommands] = new StringListValue(nameof(DescriptionQuery));
        }
    }
}