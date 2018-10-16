using Newtonsoft.Json;

namespace HomeCenter.Services.Configuration.DTO
{
    public class EventDTO : BaseDTO
    {
        [JsonProperty("SourceDeviceUid")]
        public string SourceDeviceUid { get; set; }
    }
}