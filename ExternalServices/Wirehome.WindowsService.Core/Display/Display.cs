using System;
using System.Runtime.InteropServices;
using Wirehome.WindowsService.Interop;

namespace Wirehome.WindowsService.Core
{
    public class Display : IDisplay
    {
        public Display(DisplaySettings settings)
        {
            Settings = settings;
        }

        public DisplaySettings Settings { get; private set; }

        public void Rotate(DisplayRotation newRotation)
        {
            DEVMODE mode = GetDeviceMode();
            uint temp = mode.dmPelsWidth;
            mode.dmPelsWidth = mode.dmPelsHeight;
            mode.dmPelsHeight = temp;
            mode.dmDisplayOrientation = (uint)newRotation;
            var result = Win32Api.ChangeDisplaySettings(ref mode, 0);
        }

        private static DEVMODE GetDeviceMode()
        {
            var mode = new DEVMODE();

            mode.Initialize();

            if (Win32Api.EnumDisplaySettings(null, Win32Api.ENUM_CURRENT_SETTINGS, ref mode))
                return mode;
            else
                throw new InvalidOperationException(GetLastError());
        }

        private static string GetLastError()
        {
            int err = Marshal.GetLastWin32Error();

            string msg;

            if (Win32Api.FormatMessage(
                Win32Api.FORMAT_MESSAGE_FLAGS,
                Win32Api.FORMAT_MESSAGE_FROM_HMODULE,
                (uint)err,
                0,
                out msg,
                0,
                0) == 0) return "Error";
            else return msg;
        }
    }
}
