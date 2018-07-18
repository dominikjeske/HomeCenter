using Newtonsoft.Json;
using Wirehome.Alexa.Model.Common;

namespace Wirehome.Alexa.Model.Discovery
{
    public class PayloadWithScope : Payload
    {
        [JsonProperty("scope")]
        public Scope Scope { get; set; }
    }
}
