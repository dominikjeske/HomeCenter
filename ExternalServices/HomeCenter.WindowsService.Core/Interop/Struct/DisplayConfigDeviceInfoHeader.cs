using HomeCenter.WindowsService.Core.Interop.Enum;
using System.Runtime.InteropServices;

namespace HomeCenter.WindowsService.Core.Interop.Struct
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DisplayConfigDeviceInfoHeader
    {
        public DisplayConfigDeviceInfoType type;
        public int size;
        public LUID adapterId;
        public uint id;
    }
}