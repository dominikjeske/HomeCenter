using System.Text.Json;
using System.Text.Json.Serialization;

namespace HomeCenter.Adapters.Kodi.Messages.JsonModels
{
    public class JsonRpcError
    {
        [JsonPropertyName("code")]
        public RpcErrorCode? Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public Data? Data { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}