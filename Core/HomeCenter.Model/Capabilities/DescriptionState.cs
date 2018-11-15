using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Messages.Queries.Device;

namespace HomeCenter.Model.Capabilities
{
    public class DescriptionState : State
    {
        public static string StateName { get; } = nameof(DescriptionState);

        public DescriptionState(string ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.StateName] = nameof(DescriptionState);
            this[StateProperties.CapabilityName] = Constants.Capabilities.InfoController;

            SetPropertyList(StateProperties.SupportedCommands, nameof(DescriptionQuery));

        }
    }
}