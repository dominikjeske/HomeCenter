using Newtonsoft.Json;
using System.Collections.Generic;

namespace HomeCenter.Services.Configuration.DTO
{
    public class AreaDTO : BaseDTO
    {
        [JsonProperty("Areas")]
        public IList<AreaDTO> Areas { get; set; } = new List<AreaDTO>();

        [JsonProperty("Components")]
        public IList<ComponentDTO> Components { get; set; } = new List<ComponentDTO>();

        [JsonProperty("AttachedProperties")]
        public IList<AttachedPropertyDTO> AttachedProperties { get; set; }

    }
}