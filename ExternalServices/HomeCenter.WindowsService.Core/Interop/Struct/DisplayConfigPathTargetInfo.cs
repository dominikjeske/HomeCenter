using HomeCenter.WindowsService.Core.Interop.Enum;
using System.Runtime.InteropServices;

namespace HomeCenter.WindowsService.Core.Interop.Struct
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DisplayConfigPathTargetInfo
    {
        public LUID adapterId;
        public uint id;
        public uint modeInfoIdx;
        public DisplayConfigVideoOutputTechnology outputTechnology;
        public DisplayConfigRotation rotation;
        public DisplayConfigScaling scaling;
        public DisplayConfigRational refreshRate;
        public DisplayConfigScanLineOrdering scanLineOrdering;

        public bool targetAvailable;
        public DisplayConfigTargetStatus statusFlags;
    }
}