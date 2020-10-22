using System;

namespace HomeCenter.Actors.Tests.Helpers
{
    public static class Time
    {
        public static long Tics(int miliseconds)
        {
            return TimeSpan.FromMilliseconds(miliseconds).Ticks;
        }
    }
}