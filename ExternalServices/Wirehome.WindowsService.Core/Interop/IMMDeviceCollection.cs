using System;
using System.Runtime.InteropServices;

namespace Wirehome.WindowsService.Interop
{
    [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMDeviceCollection
    {
        [PreserveSig]
        int GetCount(out int numDevices);

        [PreserveSig]
        int Item(int deviceNumber, out IMMDevice device);
    }
}
