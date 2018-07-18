using System;
using System.Collections.Generic;
using System.Text;

namespace HomeCenter.Core.Interface.Native
{
    public interface INativeTimerSerice
    {
        void CreatePeriodicTimer(Action action, TimeSpan interval);
    }
}
