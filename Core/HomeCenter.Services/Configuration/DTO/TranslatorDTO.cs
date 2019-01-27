using Newtonsoft.Json;

namespace HomeCenter.Services.Configuration.DTO
{
    public class TranslatorDTO : BaseDTO
    {
        [JsonProperty("From")]
        public ActorMessageDTO From { get; set; }
        [JsonProperty("To")]
        public ActorMessageDTO To { get; set; }
    }
}