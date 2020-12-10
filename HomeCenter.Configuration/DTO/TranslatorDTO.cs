using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HomeCenter.Services.Configuration.DTO
{
    public class TranslatorDTO : BaseDTO
    {
        public TranslatorDTO(string uid, string type, Dictionary<string, object> properties, ActorMessageDTO from, ActorMessageDTO to) : base(uid, type, properties)
        {
            From = from;
            To = to;
        }

        [JsonPropertyName("From")]
        public ActorMessageDTO From { get; set; }

        [JsonPropertyName("To")]
        public ActorMessageDTO To { get; set; }
    }
}