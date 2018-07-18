using System;

namespace Wirehome.WindowsService.Services
{
    public class ProcessDetails
    {
        public string ProcessName { get; set; }
        public int PID { get; set; }
        public IntPtr MainWindowHandle { get; set; }
    }
}
