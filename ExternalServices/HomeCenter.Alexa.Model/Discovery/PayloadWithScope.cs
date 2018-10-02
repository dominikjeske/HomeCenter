using HomeCenter.Alexa.Model.Common;
using Newtonsoft.Json;

namespace HomeCenter.Alexa.Model.Discovery
{
    public class PayloadWithScope : Payload
    {
        [JsonProperty("scope")]
        public Scope Scope { get; set; }
    }
}