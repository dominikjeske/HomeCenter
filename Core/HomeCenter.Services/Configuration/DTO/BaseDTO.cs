using HomeCenter.Broker;
using HomeCenter.Model.Core;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace HomeCenter.Services.Configuration.DTO
{
    [DebuggerDisplay("[{Uid}] [{Type}]")]
    public class BaseDTO : IBaseObject, IPropertySource
    {
        public string this[string propertyName]
        {
            get => Properties[propertyName];
            set => Properties[propertyName] = value;
        }

        public bool ContainsProperty(string propertyName) => Properties.ContainsKey(propertyName);

        [JsonProperty("Uid")]
        public string Uid { get; set; }

        [JsonProperty("Type")]
        public string Type { get; set; }

        [DefaultValue(true)]
        [JsonProperty("IsEnabled", DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool IsEnabled { get; set; }

        [JsonProperty("Properties")]
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }
}