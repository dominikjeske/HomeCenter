using HomeCenter.Model.Components;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace HomeCenter.Services.Configuration.DTO
{

    public class AttachedPropertyDTO
    {
        [DefaultValue("Service")]
        public string Service { get; set; }

        [JsonProperty("Properties")]
        public Dictionary<string, string> Properties { get; set; }

        public string AttachedActor { get; set; }

        public string AttachedArea { get; set; }
    }
}