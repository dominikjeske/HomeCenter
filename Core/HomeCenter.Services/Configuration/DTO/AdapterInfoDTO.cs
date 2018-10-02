using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace HomeCenter.Services.Configuration.DTO
{
    public class AdapterInfoDTO
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Version")]
        public Version Version { get; set; }

        [JsonProperty("Author")]
        public string Author { get; set; }

        [JsonProperty("CreationDate")]
        public DateTime CreationDate { get; set; }

        [JsonProperty("UpdateDate")]
        public DateTime UpdateDate { get; set; }

        [JsonProperty("CommonReferences")]
        public IList<string> CommonReferences { get; set; }
    }
}