using System.Collections.Generic;
using Newtonsoft.Json;

namespace HomeCenter.Core.ComponentModel.Configuration
{
    public class HomeCenterDTO
    {
        [JsonProperty("Components")]
        public IList<ComponentDTO> Components { get; set; }

        [JsonProperty("Adapters")]
        public IList<AdapterDTO> Adapters { get; set; }

        [JsonProperty("Areas")]
        public IList<AreaDTO> Areas { get; set; }
    }
}