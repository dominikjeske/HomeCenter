using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HomeCenter.Services.Configuration.DTO
{
    public class AreaDTO : DeviceActorDTO
    {
        [JsonPropertyName("Areas")]
        public IList<AreaDTO> Areas { get; set; } = new List<AreaDTO>();

        [JsonPropertyName("Components")]
        public IList<ComponentDTO> Components { get; set; } = new List<ComponentDTO>();

        [JsonPropertyName("AttachedProperties")]
        public IList<AttachedPropertyDTO> AttachedProperties { get; set; }

        public AreaDTO()
        {
            Type = "Area";
        }
    }
}