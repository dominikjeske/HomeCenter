using System;

namespace HomeCenter.Core.Interface.Native
{
    public interface INativeI2cDevice : IDisposable
    {
        NativeI2cTransferResult WritePartial(byte[] buffer);
        NativeI2cTransferResult ReadPartial(byte[] buffer);
        NativeI2cTransferResult WriteReadPartial(byte[] writeBuffer, byte[] readBuffer);
    }
}