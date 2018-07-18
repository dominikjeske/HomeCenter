using Newtonsoft.Json;
using System.Collections.Generic;
using Wirehome.ComponentModel;
using Wirehome.ComponentModel.Components;

namespace Wirehome.Core.ComponentModel.Configuration
{
    public class ComponentDTO
    {
        [JsonProperty("Uid")]
        public string Uid { get; set; }

        [JsonProperty("IsEnabled")]
        public bool IsEnabled { get; set; }

        [JsonProperty("AdapterRefs")]
        public IList<AdapterReferenceDTO> Adapters { get; set; }

        [JsonProperty("Converters")]
        [JsonConverter(typeof(ValueConverter))]
        public IDictionary<string, IValueConverter> Converters { get; set; }

        [JsonProperty("Tags")]
        public IDictionary<string, string> Tags { get; set; }

        [JsonProperty("Classes")]
        public IList<string> Classes { get; set; }

        [JsonProperty("Triggers")]
        public IList<TriggerDTO> Triggers { get; set; }

        [JsonProperty("Properties")]
        [JsonConverter(typeof(PropertyDictionaryConverter))]
        public Dictionary<string, Property> Properties { get; set; }

        [JsonProperty("Type")]
        public string Type { get; set; }
    }
}