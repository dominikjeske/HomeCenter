using System;
using System.Runtime.InteropServices;

namespace Wirehome.WindowsService.Interop
{
    [Guid("1BE09788-6894-4089-8586-9A2A6C265AC5")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMEndpoint
    {
        [PreserveSig]
        int GetDataFlow(out AudioDeviceKind dataFlow);
    }
}
