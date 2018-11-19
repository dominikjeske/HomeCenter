using HomeCenter.Model.Native;
using System;
using Windows.Devices.I2c;

namespace HomeCenter.HomeController.NativeServices
{
    internal class RaspberryI2cDevice : II2cDevice
    {
        private readonly I2cDevice _i2CDevice;

        public RaspberryI2cDevice(I2cDevice i2CDevice)
        {
            _i2CDevice = i2CDevice ?? throw new ArgumentNullException(nameof(i2CDevice));
        }

        public void Dispose() => _i2CDevice?.Dispose();

        public Model.Native.I2cTransferResult WritePartial(byte[] buffer)
        {
            var result = _i2CDevice.WritePartial(buffer);
            return new Model.Native.I2cTransferResult { BytesTransferred = result.BytesTransferred, Status = (Model.Native.I2cTransferStatus)result.Status };
        }

        public Model.Native.I2cTransferResult ReadPartial(byte[] buffer)
        {
            var result = _i2CDevice.ReadPartial(buffer);
            return new HomeCenter.Model.Native.I2cTransferResult { BytesTransferred = result.BytesTransferred, Status = (Model.Native.I2cTransferStatus)result.Status };
        }

        public HomeCenter.Model.Native.I2cTransferResult WriteReadPartial(byte[] writeBuffer, byte[] readBuffer)
        {
            var result = _i2CDevice.WriteReadPartial(writeBuffer, readBuffer);
            return new HomeCenter.Model.Native.I2cTransferResult { BytesTransferred = result.BytesTransferred, Status = (Model.Native.I2cTransferStatus)result.Status };
        }
    }
}