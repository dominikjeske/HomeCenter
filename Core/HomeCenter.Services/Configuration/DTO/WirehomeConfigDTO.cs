using Newtonsoft.Json;

namespace HomeCenter.Services.Configuration.DTO
{
    public class HomeCenterConfigDTO
    {
        [JsonProperty("HomeCenter")]
        public HomeCenterDTO HomeCenter { get; set; }
    }
}