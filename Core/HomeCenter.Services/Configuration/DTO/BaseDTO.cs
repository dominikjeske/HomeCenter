using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace HomeCenter.Services.Configuration.DTO
{
    [DebuggerDisplay("[{Uid}] [{Type}]")]
    public class BaseDTO
    {
        [JsonProperty("Uid")]
        public string Uid { get; set; }

        [JsonProperty("Type")]
        public string Type { get; set; }

        [DefaultValue(true)]
        [JsonProperty("IsEnabled", DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool IsEnabled { get; set; }

        [JsonProperty("Properties")]
        public Dictionary<string, string> Properties { get; set; }
    }
}