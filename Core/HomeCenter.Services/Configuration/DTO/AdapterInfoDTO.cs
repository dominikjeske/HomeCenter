using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HomeCenter.Services.Configuration.DTO
{
    public class AdapterInfoDTO
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("Version")]
        public Version Version { get; set; }

        [JsonPropertyName("Author")]
        public string Author { get; set; }

        [JsonPropertyName("CreationDate")]
        public DateTime CreationDate { get; set; }

        [JsonPropertyName("UpdateDate")]
        public DateTime UpdateDate { get; set; }

        [JsonPropertyName("CommonReferences")]
        public IList<string> CommonReferences { get; set; }
    }
}