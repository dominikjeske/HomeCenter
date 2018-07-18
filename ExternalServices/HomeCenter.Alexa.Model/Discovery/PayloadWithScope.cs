using Newtonsoft.Json;
using HomeCenter.Alexa.Model.Common;

namespace HomeCenter.Alexa.Model.Discovery
{
    public class PayloadWithScope : Payload
    {
        [JsonProperty("scope")]
        public Scope Scope { get; set; }
    }
}
