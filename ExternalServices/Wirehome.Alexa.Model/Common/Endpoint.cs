using Newtonsoft.Json;
using HomeCenter.Alexa.Model.Common;

namespace HomeCenter.Alexa.Model.Common
{
    public class Endpoint
    {
        [JsonProperty("scope")]
        public Scope Scope { get; set; }

        [JsonProperty("endpointId")]
        public string EndpointId { get; set; }

        [JsonProperty("cookie")]
        public Cookie Cookie { get; set; }
    }
}
