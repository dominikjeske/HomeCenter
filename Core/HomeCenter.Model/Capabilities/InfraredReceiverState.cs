using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Messages.Commands.Device;

namespace HomeCenter.Model.Capabilities
{
    
    public class InfraredReceiverState : State
    {
        public static string StateName { get; } = nameof(InfraredReceiverState);

        public InfraredReceiverState() : base(ReadWriteMode.Read)
        {
            this[StateProperties.StateName] = nameof(InfraredReceiverState);
            this[StateProperties.CapabilityName] = Constants.Capabilities.InfraredController;
        }
    }
}