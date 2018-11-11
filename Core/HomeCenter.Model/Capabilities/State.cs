using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages;
using HomeCenter.Utils.Extensions;

namespace HomeCenter.Model.Capabilities
{
    public class State : BaseObject
    {
        public State(string? ReadWriteMode = default)
        {
            if (ReadWriteMode == null) ReadWriteMode = Constants.ReadWriteMode.ReadWrite;

            SetEmptyProperty(StateProperties.TimeOfValue);
            SetEmptyProperty(StateProperties.Value);

            this[StateProperties.ReadWriteMode] = ReadWriteMode;
        }

        public bool IsCommandSupported(ActorMessage command) => AsList(StateProperties.SupportedCommands)?.Contains(command.Type) ?? false;

        public string Name => AsString(StateProperties.StateName);
        public string CapabilityName => AsString(StateProperties.CapabilityName);

        public string Value
        {
            get => this[StateProperties.Value];
            set => this[StateProperties.Value] = value;
        }

        public override string ToString() => $"{Name}: {Value} [{CapabilityName}] | Properties: [{GetProperties()?.ToFormatedString()}]";
    }
}