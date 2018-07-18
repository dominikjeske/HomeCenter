using System.Collections.Generic;

namespace HomeCenter.WindowsService.Core
{
    public interface IDisplayService
    {
        void SetDisplayMode(DisplayMode mode);
        IEnumerable<IDisplay> GetActiveMonitors();
    }
}