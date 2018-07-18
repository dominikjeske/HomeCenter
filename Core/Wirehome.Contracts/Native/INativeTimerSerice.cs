using System;
using System.Collections.Generic;
using System.Text;

namespace Wirehome.Core.Interface.Native
{
    public interface INativeTimerSerice
    {
        void CreatePeriodicTimer(Action action, TimeSpan interval);
    }
}
