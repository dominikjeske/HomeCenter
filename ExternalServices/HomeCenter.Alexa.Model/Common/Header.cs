using Newtonsoft.Json;

namespace HomeCenter.Alexa.Model.Common
{
    public class Header
    {
        [JsonProperty("namespace")]
        public string Namespace { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("payloadVersion")]
        public string PayloadVersion { get; set; } = "3";

        [JsonProperty("messageId")]
        public string MessageId { get; set; }

        [JsonProperty("correlationToken", NullValueHandling = NullValueHandling.Ignore)]
        public string CorrelationToken { get; set; }
    }
}