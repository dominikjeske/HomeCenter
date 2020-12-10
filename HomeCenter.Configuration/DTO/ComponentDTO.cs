using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace HomeCenter.Services.Configuration.DTO
{
    public class ComponentDTO : DeviceActorDTO
    {
        public ComponentDTO(string uid, string type, Dictionary<string, object> properties, IDictionary<string, string> tags, bool isEnabled, IList<AdapterReferenceDTO> adapterReferences, AdapterDTO adapter, IList<TranslatorDTO> translators, IList<TriggerDTO> triggers, string template, Dictionary<string, string> templateProperties, IList<AttachedPropertyDTO> attachedProperties) : base(uid, type, properties, tags, isEnabled)
        {
            Type = "Component";
            AdapterReferences = adapterReferences;
            Adapter = adapter;
            Translators = translators;
            Triggers = triggers;
            Template = template;
            TemplateProperties = templateProperties;
            AttachedProperties = attachedProperties;
        }

        [JsonPropertyName("AdapterRefs")]
        public IList<AdapterReferenceDTO> AdapterReferences { get; set; }

        [JsonPropertyName("Adapter")]
        public AdapterDTO Adapter { get; set; }

        [JsonPropertyName("Translators")]
        public IList<TranslatorDTO> Translators { get; set; }

        [JsonPropertyName("Triggers")]
        public IList<TriggerDTO> Triggers { get; set; }

        [DefaultValue("Template")]
        public string Template { get; set; }

        [JsonPropertyName("TemplateProperties")]
        public Dictionary<string, string> TemplateProperties { get; set; }

        [JsonPropertyName("AttachedProperties")]
        public IList<AttachedPropertyDTO> AttachedProperties { get; set; }

        //[JsonPropertyName("Converters")]
        //public IDictionary<string, IValueConverter> Converters { get; set; } = new Dictionary<string, IValueConverter>();
    }
}