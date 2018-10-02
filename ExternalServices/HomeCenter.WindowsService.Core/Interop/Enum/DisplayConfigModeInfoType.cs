using System;

namespace HomeCenter.WindowsService.Core.Interop.Enum
{
    [Flags]
    public enum DisplayConfigModeInfoType : uint
    {
        Zero = 0,

        Source = 1,
        Target = 2,
    }
}