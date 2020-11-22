using HomeCenter.Abstractions.Defaults;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HomeCenter.Abstractions
{
    public abstract class StateBase : BaseObject
    {
        protected StateBase([AllowNull] string readWriteMode = default)
        {
            if (readWriteMode == null) readWriteMode = ReadWriteMode.ReadWrite;

            this.SetProperty(StateProperties.ReadWriteMode, readWriteMode);
        }

        //TODO - check if object value in props not break state
        public string ReadWrite => this[StateProperties.ReadWriteMode].ToString();

        public string Name => this[StateProperties.StateName].ToString();
        public string CapabilityName => this[StateProperties.CapabilityName].ToString();
        public IList<string> SupportedCommands => this.AsList(StateProperties.SupportedCommands);
    }
}