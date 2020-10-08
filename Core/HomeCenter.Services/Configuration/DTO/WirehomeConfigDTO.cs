using System.Text.Json.Serialization;

namespace HomeCenter.Services.Configuration.DTO
{
    public class HomeCenterConfigDTO
    {
        [JsonPropertyName("HomeCenter")]
        public HomeCenterDTO HomeCenter { get; set; }
    }
}