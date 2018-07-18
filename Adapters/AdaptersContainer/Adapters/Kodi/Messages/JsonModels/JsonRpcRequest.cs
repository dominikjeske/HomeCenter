using Newtonsoft.Json;

namespace HomeCenter.ComponentModel.Adapters.Kodi
{
    public class JsonRpcRequest
    {
        [JsonProperty("jsonrpc", Required = Required.Always)]
        public string JsonRPC => "2.0";

        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; } = "KodiJSON-RPC";

        [JsonProperty("method", Required = Required.Always)]
        public string Method { get; set; }

        [JsonProperty("params")]
        public object Parameters { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }
    }
}