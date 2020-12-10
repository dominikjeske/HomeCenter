using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HomeCenter.Services.Configuration.DTO
{
    public class TriggerDTO
    {
        public TriggerDTO(EventDTO @event, IList<CommandDTO> commands, ScheduleDTO schedule, ConditionContainerDTO condition)
        {
            Event = @event;
            Commands = commands;
            Schedule = schedule;
            Condition = condition;
        }

        [JsonPropertyName("Event")]
        public EventDTO Event { get; set; }

        [JsonPropertyName("Commands")]
        public IList<CommandDTO> Commands { get; set; }

        [JsonPropertyName("Schedule")]
        public ScheduleDTO Schedule { get; set; }

        [JsonPropertyName("Condition")]
        public ConditionContainerDTO Condition { get; set; }
    }
}