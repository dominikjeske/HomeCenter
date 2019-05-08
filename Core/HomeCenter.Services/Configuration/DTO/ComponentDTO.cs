using HomeCenter.Model.Components;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace HomeCenter.Services.Configuration.DTO
{
    public class ComponentDTO : ActorBaseDTO
    {
        [JsonProperty("AdapterRefs")]
        public IList<AdapterReferenceDTO> AdapterReferences { get; set; } = new List<AdapterReferenceDTO>(); 

        [JsonProperty("Translators")]
        public IList<TranslatorDTO> Translators { get; set; }

        [JsonProperty("Adapter")]
        public AdapterDTO Adapter { get; set; }

        [JsonProperty("Converters")]
        [JsonConverter(typeof(ValueConverter))]
        public IDictionary<string, IValueConverter> Converters { get; set; }
        
        [JsonProperty("Triggers")]
        public IList<TriggerDTO> Triggers { get; set; }

        [DefaultValue("Template")]
        public string Template { get; set; }

        [JsonProperty("TemplateProperties")]
        public Dictionary<string, string> TemplateProperties { get; set; }

        [JsonProperty("AttachedProperties")]
        public IList<AttachedPropertyDTO> AttachedProperties { get; set; }

        public ComponentDTO()
        {
            Type = "Component";
        }

    }
}