using System.Text.Json.Serialization;

namespace HomeCenter.Adapters.Sony.Messages
{
    public class SonyAudioMuteRequest
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }

        public SonyAudioMuteRequest()
        {
        }

        public SonyAudioMuteRequest(bool @status)
        {
            Status = @status;
        }
    }
}