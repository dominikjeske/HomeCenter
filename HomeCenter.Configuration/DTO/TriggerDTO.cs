﻿using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HomeCenter.Services.Configuration.DTO
{
    public class TriggerDTO
    {
        [JsonPropertyName("Event")]
        public EventDTO? Event { get; set; }

        [JsonPropertyName("Commands")]
        public IList<CommandDTO> Commands { get; set; } = new List<CommandDTO>();

        [JsonPropertyName("Schedule")]
        public ScheduleDTO? Schedule { get; set; }

        [JsonPropertyName("Condition")]
        public ConditionContainerDTO? Condition { get; set; }
    }
}