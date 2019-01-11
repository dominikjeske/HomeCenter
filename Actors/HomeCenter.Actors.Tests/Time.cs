using System;

namespace Wirehome.Extensions.Tests
{
    public static class Time
    {
        public static long Tics(int miliseconds)
        {
            return TimeSpan.FromMilliseconds(miliseconds).Ticks;
        }
    }
}