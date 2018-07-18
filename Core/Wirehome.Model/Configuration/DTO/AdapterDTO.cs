using System.Collections.Generic;
using Newtonsoft.Json;
using Wirehome.ComponentModel;

namespace Wirehome.Core.ComponentModel.Configuration
{
    public class AdapterDTO
    {
        [JsonProperty("Uid")]
        public string Uid { get; set; }

        [JsonProperty("Type")]
        public string Type { get; set; }

        [JsonProperty("IsEnabled")]
        public bool IsEnabled { get; set; }

        [JsonProperty("Properties")]
        [JsonConverter(typeof(PropertyDictionaryConverter))]
        public Dictionary<string, Property> Properties { get; set; }

        [JsonProperty("Tags")]
        public IDictionary<string, string> Tags { get; set; }
    }
}
