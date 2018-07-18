using Newtonsoft.Json;

namespace HomeCenter.Core.ComponentModel.Configuration
{
    public class HomeCenterConfigDTO
    {
        [JsonProperty("HomeCenter")]
        public HomeCenterDTO HomeCenter { get; set; }
    }
}