using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HomeCenter.Services.Configuration.DTO
{
    public class AreaDTO : DeviceActorDTO
    {
        [JsonPropertyName("Areas")]
        public IList<AreaDTO> Areas { get; set; }

        [JsonPropertyName("Components")]
        public IList<ComponentDTO> Components { get; set; }

        [JsonPropertyName("AttachedProperties")]
        public IList<AttachedPropertyDTO> AttachedProperties { get; set; }

        public AreaDTO(string uid, string type, Dictionary<string, object> properties, IDictionary<string, string> tags, bool isEnabled, IList<AreaDTO> areas, IList<ComponentDTO> components, IList<AttachedPropertyDTO> attachedProperties) : base(uid, type, properties, tags, isEnabled)
        {
            Type = "Area";
            Areas = areas;
            Components = components;
            AttachedProperties = attachedProperties;
        }
    }
}