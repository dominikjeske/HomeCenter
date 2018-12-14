using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Core;
using System.Collections.Generic;

namespace HomeCenter.Model.Messages.Queries.Device
{
    public class DiscoveryResponse : BaseObject
    {
        public DiscoveryResponse(IList<string> requierdProperties, params State[] supportedStates)
        {
            SupportedStates = supportedStates;
            RequierdProperties = requierdProperties;
        }

        public DiscoveryResponse(params State[] supportedStates)
        {
            SupportedStates = supportedStates;
            RequierdProperties = new List<string>();
        }

        public State[] SupportedStates { get; }
        public IList<string> RequierdProperties { get; }
    }
}