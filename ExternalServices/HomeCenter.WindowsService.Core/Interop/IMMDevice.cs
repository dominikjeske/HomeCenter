using HomeCenter.WindowsService.Core.Interop.Enum;
using System;
using System.Runtime.InteropServices;

namespace HomeCenter.WindowsService.Core.Interop
{
    [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMDevice
    {
        [PreserveSig]
        int Activate(ref Guid id, ClsCtx clsCtx, IntPtr activationParams,
            [MarshalAs(UnmanagedType.IUnknown)] out object interfacePointer);

        [PreserveSig]
        int OpenPropertyStore(StorageAccessMode stgmAccess, out IPropertyStore properties);

        [PreserveSig]
        int GetId([MarshalAs(UnmanagedType.LPWStr)] out string id);

        [PreserveSig]
        int GetState(out AudioDeviceState state);
    }
}