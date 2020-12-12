using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace HomeCenter.Services.Configuration.DTO
{
    public class DeviceActorDTO : BaseDTO
    {
        public DeviceActorDTO(string uid, string type, IDictionary<string, object> properties, IDictionary<string, string> tags, bool isEnabled) : base(uid, type, properties)
        {
            Tags = tags;
            IsEnabled = isEnabled;
        }

        [JsonPropertyName("Tags")]
        public IDictionary<string, string> Tags { get; set; }

        [DefaultValue(true)]
        [JsonPropertyName("IsEnabled")]
        public bool IsEnabled { get; set; }
    }
}