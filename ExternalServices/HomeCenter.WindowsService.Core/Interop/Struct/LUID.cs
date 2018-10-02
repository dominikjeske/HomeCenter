using System.Runtime.InteropServices;

namespace HomeCenter.WindowsService.Core.Interop.Struct
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LUID
    {
        public uint LowPart;
        public uint HighPart;
    }
}