using HomeCenter.Extensions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace HomeCenter.Services.Configuration.DTO
{
    [DebuggerDisplay("[{Uid}] [{Type}]")]
    public class BaseDTO : IBaseObject, IPropertySource
    {
        public object this[string propertyName]
        {
            get => Properties[propertyName];
            set => Properties[propertyName] = value;
        }

        public bool ContainsProperty(string propertyName) => Properties.ContainsKey(propertyName);

        public IReadOnlyDictionary<string, object> GetProperties() => Properties.AsReadOnly();

        [JsonPropertyName("Uid")]
        public string Uid { get; set; } = string.Empty;

        [JsonPropertyName("Type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("Properties")]
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }
}