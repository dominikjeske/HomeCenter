using Newtonsoft.Json;
using System.Collections.Generic;

namespace HomeCenter.Services.Configuration.DTO
{
    public class AreaDTO : BaseDTO
    {
        [JsonProperty("Areas")]
        public IList<AreaDTO> Areas { get; set; }

        [JsonProperty("Components")]
        public IList<ComponentDTO> Components { get; set; }
    }
}