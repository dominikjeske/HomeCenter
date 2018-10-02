using System;

namespace HomeCenter.WindowsService.Core.Interop.Enum
{
    [Flags]
    public enum DisplayConfigSourceStatus
    {
        Zero = 0x0,
        InUse = 0x00000001
    }
}