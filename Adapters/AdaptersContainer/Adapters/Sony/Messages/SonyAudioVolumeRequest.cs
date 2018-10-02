using Newtonsoft.Json;

namespace HomeCenter.Adapters.Sony.Messages
{
    public class SonyAudioVolumeRequest
    {
        [JsonProperty("target")]
        public string Target { get; set; }

        [JsonProperty("volume")]
        public string Volume { get; set; }

        public SonyAudioVolumeRequest()
        {
        }

        public SonyAudioVolumeRequest(string @target, string @volume)
        {
            Target = @target;
            Volume = @volume;
        }
    }
}