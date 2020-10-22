using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace HomeCenter.Services.Configuration.DTO
{
    public class ComponentDTO : DeviceActorDTO
    {
        public ComponentDTO() => Type = "Component";

        [JsonPropertyName("AdapterRefs")]
        public IList<AdapterReferenceDTO> AdapterReferences { get; set; } = new List<AdapterReferenceDTO>();

        [JsonPropertyName("Adapter")]
        public AdapterDTO? Adapter { get; set; }

        [JsonPropertyName("Translators")]
        public IList<TranslatorDTO> Translators { get; set; } = new List<TranslatorDTO>();

        [JsonPropertyName("Triggers")]
        public IList<TriggerDTO> Triggers { get; set; } = new List<TriggerDTO>();

        [DefaultValue("Template")]
        public string? Template { get; set; }

        [JsonPropertyName("TemplateProperties")]
        public Dictionary<string, string> TemplateProperties { get; set; } = new Dictionary<string, string>();

        [JsonPropertyName("AttachedProperties")]
        public IList<AttachedPropertyDTO> AttachedProperties { get; set; } = new List<AttachedPropertyDTO>();

        //[JsonPropertyName("Converters")]
        //public IDictionary<string, IValueConverter> Converters { get; set; } = new Dictionary<string, IValueConverter>();
    }
}