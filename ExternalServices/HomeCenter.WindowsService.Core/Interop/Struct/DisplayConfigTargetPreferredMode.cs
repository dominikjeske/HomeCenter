using System.Runtime.InteropServices;

namespace HomeCenter.WindowsService.Core.Interop.Struct
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DisplayConfigTargetPreferredMode : IDisplayConfigInfo
    {
        public DisplayConfigDeviceInfoHeader header;
        public uint width;
        public uint height;
        public DisplayConfigTargetMode targetMode;
    }
}