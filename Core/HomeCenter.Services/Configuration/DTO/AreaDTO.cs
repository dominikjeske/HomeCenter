using Newtonsoft.Json;
using System.Collections.Generic;

namespace HomeCenter.Services.Configuration.DTO
{
    public class AreaDTO : BaseDTO
    {
        [JsonProperty("Areas")]
        public IList<AreaDTO> Areas { get; set; }

        [JsonProperty("ComponentsRefs")]
        public IList<ComponentDTO> ComponentsRefs { get; set; }

        [JsonProperty("AttachedProperties")]
        public IList<AttachedPropertyDTO> AttachedProperties { get; set; }
    }
}