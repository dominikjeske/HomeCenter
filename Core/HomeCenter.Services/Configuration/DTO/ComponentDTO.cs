using HomeCenter.Model.Components;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace HomeCenter.Services.Configuration.DTO
{
    public class ComponentDTO : ActorBaseDTO
    {
        [JsonProperty("AdapterRefs")]
        public IList<AdapterReferenceDTO> Adapters { get; set; }

        [JsonProperty("Adapter")]
        public AdapterDTO Adapter { get; set; }

        [JsonProperty("Converters")]
        [JsonConverter(typeof(ValueConverter))]
        public IDictionary<string, IValueConverter> Converters { get; set; }
        
        [JsonProperty("Triggers")]
        public IList<TriggerDTO> Triggers { get; set; }

        [DefaultValue("Component")]
        [JsonProperty("Type", DefaultValueHandling = DefaultValueHandling.Populate)]
        public new string Type { get; set; }

        [DefaultValue("Template")]
        public string Template { get; set; }

        [JsonProperty("TemplateProperties")]
        public Dictionary<string, string> TemplateProperties { get; set; }
    }
}