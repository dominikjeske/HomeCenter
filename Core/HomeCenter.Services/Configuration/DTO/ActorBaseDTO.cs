using Newtonsoft.Json;
using System.Collections.Generic;

namespace HomeCenter.Services.Configuration.DTO
{
    public class ActorBaseDTO : BaseDTO
    {
        [JsonProperty("Tags")]
        public IDictionary<string, string> Tags { get; set; }
    }
}