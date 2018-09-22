using Newtonsoft.Json;

namespace HomeCenter.Model.Adapters.Kodi
{
    public class JsonRpcError
    {
        [JsonProperty(PropertyName = "code")]
        public RpcErrorCode Code { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "data")]
        public Data Data { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }
    }
}