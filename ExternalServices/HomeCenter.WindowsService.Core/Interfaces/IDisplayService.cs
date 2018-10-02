using HomeCenter.WindowsService.Core.Display;
using System.Collections.Generic;

namespace HomeCenter.WindowsService.Core.Interfaces
{
    public interface IDisplayService
    {
        void SetDisplayMode(DisplayMode mode);

        IEnumerable<IDisplay> GetActiveMonitors();
    }
}