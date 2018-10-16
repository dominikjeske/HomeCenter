using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace HomeCenter.Services.Configuration.DTO
{
    public class ScheduleDTO : BaseDTO
    {
        [JsonProperty("CronExpression")]
        public string CronExpression { get; set; }

        [JsonProperty("Calendar")]
        public string Calendar { get; set; }

        [JsonProperty("ManualSchedules")]
        public IList<ManualScheduleDTO> ManualSchedules { get; set; }

        [JsonProperty("WorkingTime")]
        public TimeSpan? WorkingTime { get; set; }
    }
}