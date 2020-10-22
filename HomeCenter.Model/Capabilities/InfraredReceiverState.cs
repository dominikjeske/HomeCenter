using HomeCenter.Model.Capabilities.Constants;

namespace HomeCenter.Model.Capabilities
{
    public class InfraredReceiverState : StateBase
    {
        public static string StateName { get; } = nameof(InfraredReceiverState);

        public InfraredReceiverState() : base(ReadWriteMode.Read)
        {
            this[StateProperties.StateName] = nameof(InfraredReceiverState);
            this[StateProperties.CapabilityName] = Constants.Capabilities.InfraredController;
        }
    }
}