using Newtonsoft.Json;

namespace Wirehome.Alexa.Model.Common
{
    [JsonConverter(typeof(DirectiveConverter))]
    public class Directive
    {
        [JsonProperty("header")]
        public Header Header { get; set; }

        [JsonProperty("endpoint")]
        public Endpoint Endpoint { get; set; }
        
        [JsonProperty("payload")]
        public Payload Payload { get; set; }
    }
}