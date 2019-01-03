using Newtonsoft.Json;
using System.Collections.Generic;

namespace HomeCenter.Services.Configuration.DTO
{
    public class HomeCenterDTO
    {
        [JsonProperty("Components")]
        public IList<ComponentDTO> Components { get; set; }

        [JsonProperty("Adapters")]
        public IList<AdapterDTO> Adapters { get; set; }

        [JsonProperty("Areas")]
        public IList<AreaDTO> Areas { get; set; }

        [JsonProperty("Services")]
        public IList<ServiceDTO> Services { get; set; }

        [JsonProperty("Templates")]
        public IList<ComponentDTO> Templates { get; set; }
    }
}