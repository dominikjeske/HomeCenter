using Newtonsoft.Json;
using System.Collections.Generic;

namespace HomeCenter.Core.ComponentModel.Configuration
{
    public class TriggerDTO
    {
        [JsonProperty("Event")]
        public EventDTO Event { get; set; }

        [JsonProperty("Commands")]
        public IList<CommandDTO> Commands { get; set; }

        [JsonProperty("Schedule")]
        public ScheduleDTO Schedule { get; set; }

        [JsonProperty("Condition")]
        public ConditionContainerDTO Condition { get; set; }
    }
}