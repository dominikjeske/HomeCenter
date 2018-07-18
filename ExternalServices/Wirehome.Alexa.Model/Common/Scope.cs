using Newtonsoft.Json;

namespace Wirehome.Alexa.Model.Common
{
    public class Scope
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
