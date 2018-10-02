using System;

namespace HomeCenter.Model.Native
{
    public interface II2cDevice : IDisposable
    {
        I2cTransferResult WritePartial(byte[] buffer);

        I2cTransferResult ReadPartial(byte[] buffer);

        I2cTransferResult WriteReadPartial(byte[] writeBuffer, byte[] readBuffer);
    }
}