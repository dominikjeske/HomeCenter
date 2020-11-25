using System.Text.Json.Serialization;

namespace HomeCenter.Adapters.Sony.Messages
{
    public class SonyAudioVolumeRequest
    {
        [JsonPropertyName("target")]
        public string? Target { get; set; }

        [JsonPropertyName("volume")]
        public string? Volume { get; set; }

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