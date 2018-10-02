using System.Runtime.InteropServices;

namespace HomeCenter.WindowsService.Core.Interop.Struct
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DisplayConfigRational
    {
        public uint numerator;
        public uint denominator;
    }
}