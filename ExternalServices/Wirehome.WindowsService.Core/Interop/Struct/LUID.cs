using System.Runtime.InteropServices;

namespace Wirehome.WindowsService.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LUID
    {
        public uint LowPart;
        public uint HighPart;
    }
}