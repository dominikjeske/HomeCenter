using Newtonsoft.Json;

namespace Wirehome.ComponentModel.Adapters.Kodi
{
    public class JsonRpcResponse
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "jsonrpc")]
        public string JsonRpc { get; set; }

        [JsonProperty(PropertyName = "result")]
        public object Result { get; set; }

        [JsonProperty(PropertyName = "error")]
        public JsonRpcError Error { get; set; }
    }
}