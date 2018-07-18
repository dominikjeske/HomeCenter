using Newtonsoft.Json;

namespace Wirehome.Core.ComponentModel.Configuration
{
    public class TriggerDTO
    {
        [JsonProperty("Event")]
        public EventDTO Event { get; set; }

        [JsonProperty("Command")]
        public CommandDTO Command { get; set; }

        [JsonProperty("FinishCommand")]
        public CommandDTO FinishCommand { get; set; }

        [JsonProperty("Schedule")]
        public ScheduleDTO Schedule { get; set; }

        [JsonProperty("Condition")]
        public ConditionContainerDTO Condition { get; set; }
    }
}