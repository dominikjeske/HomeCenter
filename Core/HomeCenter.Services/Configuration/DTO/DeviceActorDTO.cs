using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace HomeCenter.Services.Configuration.DTO
{
    public class DeviceActorDTO : BaseDTO
    {
        [JsonPropertyName("Tags")]
        public IDictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        [DefaultValue(true)]
        [JsonPropertyName("IsEnabled")]
        public bool IsEnabled { get; set; }
    }
}