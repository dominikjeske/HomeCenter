using System.Runtime.InteropServices;

namespace HomeCenter.WindowsService.Core.Interop.Struct
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DisplayConfigTargetMode
    {
        public DisplayConfigVideoSignalInfo targetVideoSignalInfo;
    }
}