

using System.Text.Json.Serialization;

namespace HomeCenter.Adapters.Kodi.Messages.JsonModels
{
    public class JsonRpcResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("jsonrpc")]
        public string JsonRpc { get; set; }

        [JsonPropertyName("result")]
        public object Result { get; set; }

        [JsonPropertyName("error")]
        public JsonRpcError Error { get; set; }
    }
}