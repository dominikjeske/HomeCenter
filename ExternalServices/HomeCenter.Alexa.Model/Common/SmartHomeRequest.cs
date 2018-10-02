using Newtonsoft.Json;

namespace HomeCenter.Alexa.Model.Common
{
    public class SmartHomeRequest
    {
        [JsonProperty("directive")]
        public Directive Directive { get; set; }
    }
}