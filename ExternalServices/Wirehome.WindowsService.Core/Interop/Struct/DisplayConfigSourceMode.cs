using System.Runtime.InteropServices;
using Wirehome.WindowsService.Interop;

namespace Wirehome.WindowsService.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DisplayConfigSourceMode
    {
        public int width;
        public int height;
        public DisplayConfigPixelFormat pixelFormat;
        public PointL position;
    }
}