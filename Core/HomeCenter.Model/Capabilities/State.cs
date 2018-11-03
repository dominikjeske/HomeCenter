using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Core;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages;
using HomeCenter.Model.ValueTypes;
using System.Linq;

namespace HomeCenter.Model.Capabilities
{
    public class State : BaseObject
    {
        public State(StringValue ReadWriteMode = default)
        {
            if (ReadWriteMode == null) ReadWriteMode = Constants.ReadWriteMode.ReadWrite;

            this[StateProperties.TimeOfValue] = new DateTimeValue();
            this[StateProperties.ReadWriteMode] = ReadWriteMode;
        }

        public bool IsCommandSupported(ActorMessage command) => ((StringListValue)this[StateProperties.SupportedCommands]).Value.Contains(command.Type);

        public string Name => this[StateProperties.StateName].AsString();
        public string CapabilityName => this[StateProperties.CapabilityName].AsString();

        public IValue Value
        {
            get => this[StateProperties.Value];
            set => this[StateProperties.Value] = value;
        }

        public override string ToString() => $"{Name}: {Value} [{CapabilityName}]";
        
    }
}