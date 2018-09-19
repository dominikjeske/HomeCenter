using HomeCenter.ComponentModel.Capabilities.Constants;
using HomeCenter.ComponentModel.Commands;
using HomeCenter.ComponentModel.ValueTypes;
using HomeCenter.Model.ComponentModel.Capabilities.Constants;
using HomeCenter.Model.Core;
using System.Linq;

namespace HomeCenter.ComponentModel.Capabilities
{
    public class State : BaseObject
    {
        public State(StringValue ReadWriteMode = default)
        {
            if (ReadWriteMode == null) ReadWriteMode = ReadWriteModeValues.ReadWrite;

            this[StateProperties.TimeOfValue] = new DateTimeValue();
            this[StateProperties.ReadWriteMode] = ReadWriteMode;
        }

        public bool IsCommandSupported(Command command) => ((StringListValue)this[StateProperties.SupportedCommands]).Value.Contains(command.Type);
    }
}