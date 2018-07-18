using System.Runtime.InteropServices;

namespace HomeCenter.WindowsService.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PointL
    {
        public int x;
        public int y;
    }
}