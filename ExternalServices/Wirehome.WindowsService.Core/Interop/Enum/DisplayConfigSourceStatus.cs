using System;

namespace Wirehome.WindowsService.Interop
{
    [Flags]
    public enum DisplayConfigSourceStatus
    {
        Zero = 0x0,
        InUse = 0x00000001
    }
}