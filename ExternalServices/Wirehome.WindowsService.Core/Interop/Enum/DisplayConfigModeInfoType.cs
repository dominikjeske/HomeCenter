using System;

namespace Wirehome.WindowsService.Interop
{
    [Flags]
    public enum DisplayConfigModeInfoType : uint
    {
        Zero = 0,

        Source = 1,
        Target = 2,
    }
}