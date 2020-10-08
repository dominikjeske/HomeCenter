using System.Text.Json.Serialization;

namespace HomeCenter.Services.Configuration.DTO
{
    public class TranslatorDTO : BaseDTO
    {
        [JsonPropertyName("From")]
        public ActorMessageDTO From { get; set; }
        [JsonPropertyName("To")]
        public ActorMessageDTO To { get; set; }
    }
}