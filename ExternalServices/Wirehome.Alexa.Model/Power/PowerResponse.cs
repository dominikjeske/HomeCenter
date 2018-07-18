using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Wirehome.Alexa.Model.Common;

namespace Wirehome.Alexa.Model.Power
{
    public class PowerResponse
    {
        [JsonProperty("context")]
        public Context Context { get; set; }

        [JsonProperty("event")]
        public Event Event { get; set; }
    }

    public class Event
    {
        [JsonProperty("header")]
        public Header Header { get; set; }

        [JsonProperty("endpoint")]
        public SimpleEndpoint Endpoint { get; set; }

        [JsonProperty("payload")]
        public Payload Payload { get; set; }
    }

    public class Property
    {
        [JsonProperty("namespace")]
        public string Namespace { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("timeOfSample")]
        public DateTime TimeOfSample { get; set; }

        [JsonProperty("uncertaintyInMilliseconds")]
        public int UncertaintyInMilliseconds { get; set; }
    }

    public class Context
    {
        [JsonProperty("properties")]
        public IList<Property> Properties { get; set; }
    }


    public class SimpleEndpoint
    {
        [JsonProperty("scope")]
        public Scope Scope { get; set; }

        [JsonProperty("endpointId")]
        public string EndpointId { get; set; }
    }


}
