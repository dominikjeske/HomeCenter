using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HomeCenter.Services.Configuration.DTO
{
    public class ScheduleDTO : BaseDTO
    {
        [JsonPropertyName("CronExpression")]
        public string? CronExpression { get; set; }

        [JsonPropertyName("Calendar")]
        public string? Calendar { get; set; }

        [JsonPropertyName("ManualSchedules")]
        public IList<ManualScheduleDTO> ManualSchedules { get; set; } = new List<ManualScheduleDTO>();

        [JsonPropertyName("WorkingTime")]
        public TimeSpan? WorkingTime { get; set; }
    }
}