using System.Text.Json;
using System.Text.Json.Serialization;

namespace HomeCenter.Adapters.Kodi.Messages.JsonModels
{
    public class JsonRpcRequest
    {
        [JsonPropertyName("jsonrpc")]
        public string JsonRPC => "2.0";

        [JsonPropertyName("id")]
        public string Id { get; set; } = "KodiJSON-RPC";

        [JsonPropertyName("method")]
        public string Method { get; set; }

        [JsonPropertyName("params")]
        public object Parameters { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}