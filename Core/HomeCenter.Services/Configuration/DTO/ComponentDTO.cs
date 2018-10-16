using HomeCenter.Model.Components;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace HomeCenter.Services.Configuration.DTO
{
    public class ComponentDTO : BaseDTO
    {
        [JsonProperty("AdapterRefs")]
        public IList<AdapterReferenceDTO> Adapters { get; set; }

        [JsonProperty("Converters")]
        [JsonConverter(typeof(ValueConverter))]
        public IDictionary<string, IValueConverter> Converters { get; set; }

        [JsonProperty("Classes")]
        public IList<string> Classes { get; set; }

        [JsonProperty("Triggers")]
        public IList<TriggerDTO> Triggers { get; set; }

        [DefaultValue("Component")]
        [JsonProperty("Type", DefaultValueHandling = DefaultValueHandling.Populate)]
        public new string Type { get; set; }
    }
}