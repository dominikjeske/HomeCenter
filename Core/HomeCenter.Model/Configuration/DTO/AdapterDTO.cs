﻿using System.Collections.Generic;
using Newtonsoft.Json;
using HomeCenter.ComponentModel;
using System.ComponentModel;

namespace HomeCenter.Core.ComponentModel.Configuration
{
    public class AdapterDTO
    {
        [JsonProperty("Uid")]
        public string Uid { get; set; }

        [JsonProperty("Type")]
        public string Type { get; set; }

        [DefaultValue(true)]
        [JsonProperty("IsEnabled", DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool IsEnabled { get; set; }

        [JsonProperty("Properties")]
        [JsonConverter(typeof(PropertyDictionaryConverter))]
        public Dictionary<string, Property> Properties { get; set; }

        [JsonProperty("Tags")]
        public IDictionary<string, string> Tags { get; set; }
    }
}