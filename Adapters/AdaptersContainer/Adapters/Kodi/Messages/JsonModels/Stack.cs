using Newtonsoft.Json;

namespace Wirehome.ComponentModel.Adapters.Kodi
{
    public class Stack
    {
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "type")]
        public object Type { get; set; }
    }
}