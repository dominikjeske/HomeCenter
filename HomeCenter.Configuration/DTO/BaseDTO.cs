using HomeCenter.Abstractions;
using HomeCenter.EventAggregator;
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

        public BaseDTO(string uid, string type, IDictionary<string, object> properties)
        {
            Uid = uid;
            Type = type;
            Properties = new Dictionary<string, object>(properties);
        }

        public bool ContainsProperty(string propertyName) => Properties.ContainsKey(propertyName);

        public IReadOnlyDictionary<string, object> GetProperties() => Properties.AsReadOnly();

        [JsonPropertyName("Uid")]
        public string Uid { get; set; }

        [JsonPropertyName("Type")]
        public string Type { get; set; }

        [JsonPropertyName("Properties")]
        public Dictionary<string, object> Properties { get; set; }
    }
}