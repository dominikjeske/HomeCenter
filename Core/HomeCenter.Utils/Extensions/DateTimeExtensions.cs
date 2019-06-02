using System;

namespace HomeCenter.Utils.Extensions
{
    public static class DateTimeExtensions
    {
        public static TimeSpan Between(this DateTimeOffset currentPointInTime, DateTimeOffset previousPointInTime) => currentPointInTime - previousPointInTime;

        public static bool LastedLessThen(this TimeSpan measuredTime, TimeSpan comparedTime)
        {
            //if (measuredTime == null) return false;
            if (measuredTime > comparedTime) return false;
            return true;
        }

        /// <summary>
        /// Checks is move is physically possible by person to move from one room to other
        /// </summary>
        /// <param name="measuredTime"></param>
        /// <param name="timeWindow"></param>
        /// <returns></returns>
        public static bool IsPossible(this TimeSpan measuredTime, TimeSpan timeWindow) => measuredTime >= timeWindow;
        

        public static bool HappendBeforePrecedingTimeWindow(this DateTimeOffset time, DateTimeOffset? comparedTime, TimeSpan timeWindow) => comparedTime < time && time - comparedTime > timeWindow;

        
        public static TimeSpan IncreaseByPercentage(this TimeSpan time, double percentage) => TimeSpan.FromTicks(time.Ticks + (long)(time.Ticks * (percentage / 100.0)));

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