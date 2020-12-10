using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HomeCenter.Services.Configuration.DTO
{
    public class HomeCenterDTO
    {
        public HomeCenterDTO(IList<AdapterDTO> sharedAdapters, AreaDTO mainArea, IList<ServiceDTO> services, IList<ComponentDTO> templates)
        {
            SharedAdapters = sharedAdapters;
            MainArea = mainArea;
            Services = services;
            Templates = templates;
        }

        [JsonPropertyName("SharedAdapters")]
        public IList<AdapterDTO> SharedAdapters { get; set; }

        [JsonPropertyName("MainArea")]
        public AreaDTO MainArea { get; set; }

        [JsonPropertyName("Services")]
        public IList<ServiceDTO> Services { get; set; }

        [JsonPropertyName("Templates")]
        public IList<ComponentDTO> Templates { get; set; }
    }
}