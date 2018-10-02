using System;

namespace HomeCenter.WindowsService.Core
{
    public class ProcessDetails
    {
        public string ProcessName { get; set; }
        public int PID { get; set; }
        public IntPtr MainWindowHandle { get; set; }
    }
}