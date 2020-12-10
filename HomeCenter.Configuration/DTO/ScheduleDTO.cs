using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HomeCenter.Services.Configuration.DTO
{
    public class ScheduleDTO : BaseDTO
    {
        public ScheduleDTO(string uid, string type, Dictionary<string, object> properties, string cronExpression, string calendar, IList<ManualScheduleDTO> manualSchedules, TimeSpan workingTime) : base(uid, type, properties)
        {
            CronExpression = cronExpression;
            Calendar = calendar;
            ManualSchedules = manualSchedules;
            WorkingTime = workingTime;
        }

        [JsonPropertyName("CronExpression")]
        public string CronExpression { get; set; }

        [JsonPropertyName("Calendar")]
        public string Calendar { get; set; }

        [JsonPropertyName("ManualSchedules")]
        public IList<ManualScheduleDTO> ManualSchedules { get; set; }

        [JsonPropertyName("WorkingTime")]
        public TimeSpan WorkingTime { get; set; }
    }
}