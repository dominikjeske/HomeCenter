using Newtonsoft.Json;

namespace HomeCenter.Model.Adapters.Sony
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