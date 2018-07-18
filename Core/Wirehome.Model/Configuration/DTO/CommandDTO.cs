using Newtonsoft.Json;
using System.Collections.Generic;
using Wirehome.ComponentModel;

namespace Wirehome.Core.ComponentModel.Configuration
{
    public class CommandDTO
    {
        [JsonProperty("Uid")]
        public string Uid { get; set; }

        [JsonProperty("Type")]
        public string Type { get; set; }

        [JsonProperty("Tags")]
        public IDictionary<string, string> Tags { get; set; }

        [JsonProperty("Properties")]
        [JsonConverter(typeof(PropertyDictionaryConverter))]
        public Dictionary<string, Property> Properties { get; set; }
    }
}