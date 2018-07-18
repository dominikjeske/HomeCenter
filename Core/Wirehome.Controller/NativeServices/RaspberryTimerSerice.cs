using System;
using Windows.System.Threading;
using HomeCenter.Core.Interface.Native;

namespace HomeCenter.Raspberry
{
    internal class RaspberryTimerSerice : INativeTimerSerice
    {
        public void CreatePeriodicTimer(Action action, TimeSpan interval) => ThreadPoolTimer.CreatePeriodicTimer(x => action(), interval);
    }
}
