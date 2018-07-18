using System.Collections.Generic;
using Wirehome.ComponentModel.Capabilities;

namespace Wirehome.ComponentModel.Commands.Responses
{
    public class DiscoveryResponse : BaseObject
    {
        public DiscoveryResponse(IList<string> requierdProperties, IList<EventSource> eventSources, params State[] supportedStates)
        {
            SupportedStates = supportedStates;
            RequierdProperties = requierdProperties;
            EventSources = eventSources;
        }

        public DiscoveryResponse(IList<string> requierdProperties, params State[] supportedStates)
        {
            SupportedStates = supportedStates;
            RequierdProperties = requierdProperties;
            EventSources = new List<EventSource>();
        }

        public DiscoveryResponse(IList<EventSource> eventSources, params State[] supportedStates)
        {
            SupportedStates = supportedStates;
            RequierdProperties = new List<string>();
            EventSources = eventSources;
        }

        public DiscoveryResponse(params State[] supportedStates)
        {
            SupportedStates = supportedStates;
            RequierdProperties = new List<string>();
            EventSources = new List<EventSource>();
        }

        public State[] SupportedStates { get; }
        public IList<string> RequierdProperties { get; }
        public IList<EventSource> EventSources { get; }
    }
}
