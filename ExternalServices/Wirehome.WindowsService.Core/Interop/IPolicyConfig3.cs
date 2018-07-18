using System.Runtime.InteropServices;

namespace Wirehome.WindowsService.Interop
{
    [ComImport]
    [Guid("00000000-0000-0000-C000-000000000046")]  // We just cast it to IUnknown, and pray that v-table layout is correct
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPolicyConfig3
    {
        void Reserved1();

        void Reserved2();

        void Reserved3();

        void Reserved4();

        void Reserved5();

        void Reserved6();

        void Reserved7();

        void Reserved8();

        void Reserved9();

        void Reserved10();

        [PreserveSig]
        int SetDefaultEndpoint(string deviceId, AudioDeviceRole role);

        void Reserved11();
    }
}