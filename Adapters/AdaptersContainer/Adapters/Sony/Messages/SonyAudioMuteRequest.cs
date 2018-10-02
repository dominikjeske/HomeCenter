using Newtonsoft.Json;

namespace HomeCenter.Adapters.Sony.Messages
{
    public class SonyAudioMuteRequest
    {
        [JsonProperty("status")]
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