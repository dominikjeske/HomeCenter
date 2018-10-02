using Newtonsoft.Json;

namespace HomeCenter.Alexa.Model.Common
{
    public class Scope
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }
    }
}