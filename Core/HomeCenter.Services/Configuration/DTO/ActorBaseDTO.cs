using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HomeCenter.Services.Configuration.DTO
{
    public class ActorBaseDTO : BaseDTO
    {
        [JsonPropertyName("Tags")]
        public IDictionary<string, string> Tags { get; set; }
    }
}