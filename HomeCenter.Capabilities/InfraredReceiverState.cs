using HomeCenter.Abstractions;
using HomeCenter.Abstractions.Defaults;

namespace HomeCenter.Capabilities
{
    public class InfraredReceiverState : StateBase
    {
        public static string StateName { get; } = nameof(InfraredReceiverState);

        public InfraredReceiverState() : base(ReadWriteMode.Read)
        {
            this[StateProperties.StateName] = nameof(InfraredReceiverState);
            this[StateProperties.CapabilityName] = Abstractions.Defaults.Capabilities.InfraredController;
        }
    }
}