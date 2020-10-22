using HomeCenter.Abstractions;
using HomeCenter.Abstractions.Defaults;
using HomeCenter.Messages.Commands.Device;

namespace HomeCenter.Capabilities
{
    public class InfraredSenderState : StateBase
    {
        public static string StateName { get; } = nameof(InfraredSenderState);

        public InfraredSenderState() : base(ReadWriteMode.Write)
        {
            this[StateProperties.StateName] = nameof(InfraredSenderState);
            this[StateProperties.CapabilityName] = Abstractions.Defaults.Capabilities.InfraredController;
            this.SetPropertyList(StateProperties.SupportedCommands, nameof(SendCodeCommand));
        }
    }
}