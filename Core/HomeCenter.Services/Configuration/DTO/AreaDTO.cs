﻿using HomeCenter.Model.Core;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace HomeCenter.Services.Configuration.DTO
{
    public class AreaDTO
    {
        [JsonProperty("Uid")]
        public string Uid { get; set; }

        [JsonProperty("Areas")]
        public IList<AreaDTO> Areas { get; set; }

        [JsonProperty("Components")]
        public IList<ComponentDTO> Components { get; set; }

        [JsonProperty("Tags")]
        public IDictionary<string, string> Tags { get; set; }

        [JsonProperty("Properties")]
        [JsonConverter(typeof(PropertyDictionaryConverter))]
        public Dictionary<string, Property> Properties { get; set; }

        [JsonProperty("Type")]
        public string Type { get; set; }
    }
}