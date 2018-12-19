using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Messages.Commands.Device;

namespace HomeCenter.Model.Capabilities
{
    public class PowerState : State
    {
        public static string StateName { get; } = nameof(PowerState);

        public PowerState(string ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.StateName] = nameof(PowerState);
            this[StateProperties.CapabilityName] = Constants.Capabilities.PowerController;
            SetPropertyList(StateProperties.SupportedCommands, nameof(TurnOnCommand), nameof(TurnOffCommand), nameof(RefreshCommand)); //TODO set Refresh to default on all states?
        }
    }
}