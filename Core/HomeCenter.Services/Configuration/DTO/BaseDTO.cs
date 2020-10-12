using HomeCenter.Broker;
using HomeCenter.Model.Core;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace HomeCenter.Services.Configuration.DTO
{
    [DebuggerDisplay("[{Uid}] [{Type}]")]
    public class BaseDTO : IBaseObject, IPropertySource
    {
        public string this[string propertyName]
        {
            get => Properties[propertyName].ToString();
            set => Properties[propertyName] = value;
        }

        public bool ContainsProperty(string propertyName) => Properties.ContainsKey(propertyName);

        [JsonPropertyName("Uid")]
        public string Uid { get; set; } = "";

        [JsonPropertyName("Type")]
        public string Type { get; set; } = "";

        [JsonPropertyName("Properties")]
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }
}