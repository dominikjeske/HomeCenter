using Quartz;
using System;

namespace Wirehome.Model.Extensions
{
    public static class TimeSpanExtensions
    {
        public static TimeOfDay ToTimeOfDay(this TimeSpan time)
        {
            return new TimeOfDay(time.Hours, time.Minutes, time.Seconds);
        }
    }
}