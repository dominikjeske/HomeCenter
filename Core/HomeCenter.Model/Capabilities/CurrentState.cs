
using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Messages.Commands.Device;

namespace HomeCenter.Model.Capabilities
{

    public class CurrentState : StateBase
    {
        public static string StateName { get; } = nameof(CurrentState);

        public CurrentState() : base(ReadWriteMode.Read)
        {
            this[StateProperties.StateName] = nameof(CurrentState);
            this[StateProperties.CapabilityName] = Constants.Capabilities.PowerController;
        }
    }
}