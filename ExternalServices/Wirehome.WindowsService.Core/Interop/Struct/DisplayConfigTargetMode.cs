using System.Runtime.InteropServices;

namespace Wirehome.WindowsService.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DisplayConfigTargetMode
    {
        public DisplayConfigVideoSignalInfo targetVideoSignalInfo;
    }
}