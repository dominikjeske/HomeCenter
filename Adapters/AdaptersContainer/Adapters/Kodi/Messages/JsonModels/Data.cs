using Newtonsoft.Json;

namespace Wirehome.ComponentModel.Adapters.Kodi
{
    public class Data
    {
        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "stack")]
        public Stack Stack { get; set; }
    }
}