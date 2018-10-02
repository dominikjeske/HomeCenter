using HomeCenter.WindowsService.Core.Interop.Enum;
using System.Runtime.InteropServices;

namespace HomeCenter.WindowsService.Core.Interop.Struct
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DisplayConfigPathSourceInfo
    {
        public LUID adapterId;
        public uint id;
        public uint modeInfoIdx;

        public DisplayConfigSourceStatus statusFlags;
    }
}