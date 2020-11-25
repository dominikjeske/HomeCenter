using System.Text.Json.Serialization;

namespace HomeCenter.Adapters.Kodi.Messages.JsonModels
{
    public class Data
    {
        [JsonPropertyName("method")]
        public string? Method { get; set; }

        [JsonPropertyName("stack")]
        public Stack? Stack { get; set; }
    }
}