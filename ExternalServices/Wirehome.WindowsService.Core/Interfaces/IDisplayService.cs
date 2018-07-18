using System.Collections.Generic;

namespace Wirehome.WindowsService.Core
{
    public interface IDisplayService
    {
        void SetDisplayMode(DisplayMode mode);
        IEnumerable<IDisplay> GetActiveMonitors();
    }
}