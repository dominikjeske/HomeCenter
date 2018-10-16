using Newtonsoft.Json;

namespace HomeCenter.Adapters.Kodi.Messages.JsonModels
{
    public class Data
    {
        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "stack")]
        public Stack Stack { get; set; }
    }
}