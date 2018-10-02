using HomeCenter.WindowsService.Core.Interop.Struct;
using System;
using System.Runtime.InteropServices;

namespace HomeCenter.WindowsService.Core.Interop
{
    [Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPropertyStore
    {
        [PreserveSig]
        int GetCount(out int propCount);

        [PreserveSig]
        int GetAt(int property, out PropertyKey key);

        [PreserveSig]
        int GetValue(ref PropertyKey key, out PropVariant value);

        [PreserveSig]
        int SetValue(ref PropertyKey key, ref PropVariant value);

        [PreserveSig]
        int Commit();
    }
}