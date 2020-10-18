using System;

namespace HomeCenter.Services.MotionService.Tests
{
    public static class Time
    {
        public static long Tics(int miliseconds)
        {
            return TimeSpan.FromMilliseconds(miliseconds).Ticks;
        }
    }
}