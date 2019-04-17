using Newtonsoft.Json;
using System.Collections.Generic;

namespace HomeCenter.Services.Configuration.DTO
{
    public class HomeCenterDTO
    {
        [JsonProperty("SharedAdapters")]
        public IList<AdapterDTO> SharedAdapters { get; set; } = new List<AdapterDTO>();

        [JsonProperty("MainArea")]
        public AreaDTO MainArea { get; set; }

        [JsonProperty("Services")]
        public IList<ServiceDTO> Services { get; set; } = new List<ServiceDTO>();

        [JsonProperty("Templates")]
        public IList<ComponentDTO> Templates { get; set; } = new List<ComponentDTO>();
    }
}