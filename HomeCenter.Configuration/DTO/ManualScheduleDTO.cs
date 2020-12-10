using System;
using System.Text.Json.Serialization;

namespace HomeCenter.Services.Configuration.DTO
{
    public class ManualScheduleDTO
    {
        public ManualScheduleDTO(TimeSpan start, TimeSpan finish, TimeSpan workingTime)
        {
            Start = start;
            Finish = finish;
            WorkingTime = workingTime;
        }

        [JsonPropertyName("Start")]
        public TimeSpan Start { get; set; }

        [JsonPropertyName("Finish")]
        public TimeSpan Finish { get; set; }

        [JsonPropertyName("WorkingTime")]
        public TimeSpan WorkingTime { get; set; }
    }
}