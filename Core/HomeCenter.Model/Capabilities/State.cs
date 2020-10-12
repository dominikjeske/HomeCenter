using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Core;
using System.Collections.Generic;

namespace HomeCenter.Model.Capabilities
{
    public abstract class StateBase : BaseObject
    {
        protected StateBase(string readWriteMode = default)
        {
            if (readWriteMode == null) readWriteMode = ReadWriteMode.ReadWrite;

            this.SetProperty(StateProperties.ReadWriteMode, readWriteMode);
        }

        public string ReadWrite => this[StateProperties.ReadWriteMode];
        public string Name => this[StateProperties.StateName];
        public string CapabilityName => this[StateProperties.CapabilityName];
        public IList<string> SupportedCommands => this.AsList(StateProperties.SupportedCommands);
    }
}