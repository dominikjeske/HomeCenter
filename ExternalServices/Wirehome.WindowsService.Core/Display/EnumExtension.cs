using Wirehome.WindowsService.Interop;

namespace Wirehome.WindowsService.Core
{
    public static class EnumExtension
    {
        public static DisplayRotation ToScreenRotation(this DisplayConfigRotation destEnum)
        {
            DisplayRotation toSourceEnum = 0x0;
            if ((destEnum & DisplayConfigRotation.Identity) == DisplayConfigRotation.Identity)
                toSourceEnum |= DisplayRotation.Default;

            if ((destEnum & DisplayConfigRotation.Rotate180) == DisplayConfigRotation.Rotate180)
                toSourceEnum |= DisplayRotation.Rotated180;

            if ((destEnum & DisplayConfigRotation.Rotate270) == DisplayConfigRotation.Rotate270)
                toSourceEnum |= DisplayRotation.Rotated270;

            if ((destEnum & DisplayConfigRotation.Rotate90) == DisplayConfigRotation.Rotate90)
                toSourceEnum |= DisplayRotation.Rotated90;

            return toSourceEnum;
        }
    }
}
