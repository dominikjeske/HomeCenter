using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HomeCenter.Services.Configuration.DTO
{
    public class HomeCenterDTO
    {
        [JsonPropertyName("SharedAdapters")]
        public IList<AdapterDTO> SharedAdapters { get; set; } = new List<AdapterDTO>();

        [JsonPropertyName("MainArea")]
        public AreaDTO? MainArea { get; set; }

        [JsonPropertyName("Services")]
        public IList<ServiceDTO> Services { get; set; } = new List<ServiceDTO>();

        [JsonPropertyName("Templates")]
        public IList<ComponentDTO> Templates { get; set; } = new List<ComponentDTO>();
    }
}