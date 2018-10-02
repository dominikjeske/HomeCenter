using System.Runtime.InteropServices;

namespace HomeCenter.WindowsService.Core.Interop.Struct
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DisplayConfig2DRegion
    {
        public uint cx;
        public uint cy;
    }
}