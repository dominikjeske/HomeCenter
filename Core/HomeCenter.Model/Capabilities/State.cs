using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages;
using HomeCenter.Model.ValueTypes;
using System.Linq;

namespace HomeCenter.Model.Capabilities
{
    public class State : BaseObject
    {
        public State(StringValue ReadWriteMode = default)
        {
            if (ReadWriteMode == null) ReadWriteMode = ReadWriteModeValues.ReadWrite;

            this[StateProperties.TimeOfValue] = new DateTimeValue();
            this[StateProperties.ReadWriteMode] = ReadWriteMode;
        }

        public bool IsCommandSupported(ActorMessage command) => ((StringListValue)this[StateProperties.SupportedCommands]).Value.Contains(command.Type);
    }
}