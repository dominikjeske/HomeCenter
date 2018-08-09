using System;

namespace HomeCenter.Core.Interface.Native
{
    public interface II2cDevice : IDisposable
    {
        I2cTransferResult WritePartial(byte[] buffer);
        I2cTransferResult ReadPartial(byte[] buffer);
        I2cTransferResult WriteReadPartial(byte[] writeBuffer, byte[] readBuffer);
    }
}