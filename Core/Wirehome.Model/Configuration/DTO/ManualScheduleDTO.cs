using Newtonsoft.Json;
using System;

namespace Wirehome.Core.ComponentModel.Configuration
{
    public class ManualScheduleDTO
    {
        [JsonProperty("Start")]
        public TimeSpan Start { get; set; }

        [JsonProperty("Finish")]
        public TimeSpan Finish { get; set; }

        [JsonProperty("WorkingTime")]
        public TimeSpan WorkingTime { get; set; }
    }
}