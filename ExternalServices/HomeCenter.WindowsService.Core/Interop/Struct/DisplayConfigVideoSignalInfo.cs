using HomeCenter.WindowsService.Core.Interop.Enum;
using System.Runtime.InteropServices;

namespace HomeCenter.WindowsService.Core.Interop.Struct
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DisplayConfigVideoSignalInfo
    {
        public long pixelRate;
        public DisplayConfigRational hSyncFreq;
        public DisplayConfigRational vSyncFreq;
        public DisplayConfig2DRegion activeSize;
        public DisplayConfig2DRegion totalSize;

        public D3DmdtVideoSignalStandard videoStandard;
        public DisplayConfigScanLineOrdering scanLineOrdering;
    }
}