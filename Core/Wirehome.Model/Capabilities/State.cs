using System.Linq;
using Wirehome.ComponentModel.Capabilities.Constants;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Components;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Model.ComponentModel.Capabilities.Constants;

namespace Wirehome.ComponentModel.Capabilities
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
