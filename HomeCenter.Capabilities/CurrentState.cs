using HomeCenter.Abstractions;
using HomeCenter.Abstractions.Defaults;

namespace HomeCenter.Capabilities
{
    public class CurrentState : StateBase
    {
        public static string StateName { get; } = nameof(CurrentState);

        public CurrentState() : base(ReadWriteMode.Read)
        {
            this[StateProperties.StateName] = nameof(CurrentState);
            this[StateProperties.CapabilityName] = Abstractions.Defaults.Capabilities.PowerController;
        }
    }
}