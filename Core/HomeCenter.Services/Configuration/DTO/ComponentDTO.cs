using HomeCenter.Model.Components;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace HomeCenter.Services.Configuration.DTO
{
    public class ComponentDTO : DeviceActorDTO
    {
        [JsonPropertyName("AdapterRefs")]
        public IList<AdapterReferenceDTO> AdapterReferences { get; set; } = new List<AdapterReferenceDTO>(); 

        [JsonPropertyName("Translators")]
        public IList<TranslatorDTO> Translators { get; set; }

        [JsonPropertyName("Adapter")]
        public AdapterDTO Adapter { get; set; }

        [JsonPropertyName("Converters")]
        //[JsonConverter(typeof(ValueConverter))] //TODO DNF
        public IDictionary<string, IValueConverter> Converters { get; set; }
        
        [JsonPropertyName("Triggers")]
        public IList<TriggerDTO> Triggers { get; set; }

        [DefaultValue("Template")]
        public string Template { get; set; }

        [JsonPropertyName("TemplateProperties")]
        public Dictionary<string, string> TemplateProperties { get; set; }

        [JsonPropertyName("AttachedProperties")]
        public IList<AttachedPropertyDTO> AttachedProperties { get; set; }

        public ComponentDTO()
        {
            Type = "Component";
        }

    }
}