using System.Runtime.InteropServices;
using Wirehome.WindowsService.Interop;

namespace Wirehome.WindowsService.Interop
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