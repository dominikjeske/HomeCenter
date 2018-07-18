using System;

namespace Wirehome.WindowsService.Interop
{
    [Flags]
    public enum DisplayConfigFlags : uint
    {
        Zero = 0x0,
        PathActive = 0x00000001
    }
}