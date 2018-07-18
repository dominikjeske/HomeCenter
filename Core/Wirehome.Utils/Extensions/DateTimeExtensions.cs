using System;

namespace Wirehome.Model.Extensions
{
    public static class DateTimeExtensions
   {
        public static bool HappendInPrecedingTimeWindow(this DateTimeOffset time, DateTimeOffset? comparedTime, TimeSpan timeWindow) => comparedTime < time && time - comparedTime < timeWindow;

        public static bool HappendBeforePrecedingTimeWindow(this DateTimeOffset time, DateTimeOffset? comparedTime, TimeSpan timeWindow) => comparedTime < time && time - comparedTime > timeWindow;

        public static bool IsMovePhisicallyPosible(this DateTimeOffset time, DateTimeOffset comparedTime, TimeSpan motionMinDiff) => TimeSpan.FromTicks(Math.Abs(time.Ticks - comparedTime.Ticks)) >= motionMinDiff;

        public static TimeSpan IncreaseByPercentage(this TimeSpan time, float percentage) => TimeSpan.FromTicks(time.Ticks + (long)(time.Ticks * (percentage / 100.0)));

        public static bool IsTimeInRange(this TimeSpan time, TimeSpan from, TimeSpan until)
        {
            if (from < until)
            {
                return time >= from && time <= until;
            }

            return time >= from || time <= until;
        }
    }
}
