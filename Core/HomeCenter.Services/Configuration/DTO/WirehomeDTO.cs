using Newtonsoft.Json;
using System.Collections.Generic;

namespace HomeCenter.Services.Configuration.DTO
{
    public class HomeCenterDTO
    {
        [JsonProperty("Components")]
        public IList<ComponentDTO> Components { get; set; } = new List<ComponentDTO>();

        [JsonProperty("Adapters")]
        public IList<AdapterDTO> Adapters { get; set; } = new List<AdapterDTO>();

        [JsonProperty("Areas")]
        public IList<AreaDTO> Areas { get; set; } = new List<AreaDTO>();

        [JsonProperty("Services")]
        public IList<ServiceDTO> Services { get; set; } = new List<ServiceDTO>();

        [JsonProperty("Templates")]
        public IList<ComponentDTO> Templates { get; set; } = new List<ComponentDTO>();
    }
}