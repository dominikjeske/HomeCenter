using Newtonsoft.Json;

namespace Wirehome.Alexa.Model.Common
{
    public class SmartHomeRequest
    {
        [JsonProperty("directive")]
        public Directive Directive { get; set; }
    }
}
