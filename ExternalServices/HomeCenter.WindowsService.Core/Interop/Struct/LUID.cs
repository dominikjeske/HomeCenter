using System.Runtime.InteropServices;

namespace HomeCenter.WindowsService.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LUID
    {
        public uint LowPart;
        public uint HighPart;
    }
}