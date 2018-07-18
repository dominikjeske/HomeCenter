using System;
using System.Threading;

namespace Wirehome.Core
{
    public static class SystemTime
    {
        private static readonly ThreadLocal<Func<DateTimeOffset>> _timeFunc = 
                                new ThreadLocal<Func<DateTimeOffset>>(() => () => DateTimeOffset.Now);

        public static DateTimeOffset Today => _timeFunc.Value().Date;
        public static DateTimeOffset Now => _timeFunc.Value();
        public static DateTimeOffset UtcNow => _timeFunc.Value().ToUniversalTime();
        public static void Reset() => _timeFunc.Value = () => DateTimeOffset.Now;
        public static void Set(DateTimeOffset time) => _timeFunc.Value = () => time;
        public static void Set(Func<DateTimeOffset> timeFunc) => _timeFunc.Value = timeFunc;
    }
}
