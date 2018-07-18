using Newtonsoft.Json;

namespace Wirehome.Core.ComponentModel.Configuration
{
    public class WirehomeConfigDTO
    {
        [JsonProperty("Wirehome")]
        public WirehomeDTO Wirehome { get; set; }
    }
}