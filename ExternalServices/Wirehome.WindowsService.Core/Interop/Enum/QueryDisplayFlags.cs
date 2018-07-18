using System;

namespace Wirehome.WindowsService.Interop
{
    [Flags]
    public enum QueryDisplayFlags : uint
    {
        Zero = 0x0,

        AllPaths = 0x00000001,
        OnlyActivePaths = 0x00000002,
        DatabaseCurrent = 0x00000004
    }
}