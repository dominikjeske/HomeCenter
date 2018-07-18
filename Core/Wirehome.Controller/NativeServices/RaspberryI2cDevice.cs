using System;
using Windows.Devices.I2c;
using Wirehome.Core.Interface.Native;

namespace Wirehome.Raspberry
{
    internal class RaspberryI2cDevice : INativeI2cDevice
    {
        private readonly I2cDevice _i2CDevice;
 
        public RaspberryI2cDevice(I2cDevice i2CDevice)
        {
            _i2CDevice = i2CDevice ?? throw new ArgumentNullException(nameof(i2CDevice));
        }
        public void Dispose() => _i2CDevice?.Dispose();

        public NativeI2cTransferResult WritePartial(byte[] buffer)
        {
            var result = _i2CDevice.WritePartial(buffer);
            return new NativeI2cTransferResult { BytesTransferred = result.BytesTransferred, Status = (NativeI2cTransferStatus)result.Status };
        }

        public NativeI2cTransferResult ReadPartial(byte[] buffer)
        {
            var result = _i2CDevice.ReadPartial(buffer);
            return new NativeI2cTransferResult { BytesTransferred = result.BytesTransferred, Status = (NativeI2cTransferStatus)result.Status };
        }

        public NativeI2cTransferResult WriteReadPartial(byte[] writeBuffer, byte[] readBuffer)
        {
            var result = _i2CDevice.WriteReadPartial(writeBuffer, readBuffer);
            return new NativeI2cTransferResult { BytesTransferred = result.BytesTransferred, Status = (NativeI2cTransferStatus)result.Status };
        }
    }
}
