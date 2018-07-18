using System.Runtime.InteropServices;

namespace Wirehome.WindowsService.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PointL
    {
        public int x;
        public int y;
    }
}