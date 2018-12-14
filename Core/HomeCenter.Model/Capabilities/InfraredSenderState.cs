using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Messages.Commands.Device;

namespace HomeCenter.Model.Capabilities
{


    public class InfraredSenderState : State
    {
        public static string StateName { get; } = nameof(InfraredSenderState);

        public InfraredSenderState() : base(ReadWriteMode.Write)
        {
            this[StateProperties.StateName] = nameof(InfraredSenderState);
            this[StateProperties.CapabilityName] = Constants.Capabilities.InfraredController;
            SetPropertyList(StateProperties.SupportedCommands, nameof(SendCodeCommand));
        }
    }
}