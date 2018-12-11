using HomeCenter.Model.Devices;
using System;

namespace HomeCenter.Services.Devices
{
    public class I2cBus : II2cBus
    {
        private const int BusId = 1;

        public void Write(int address, Span<byte> data)
        {
            //TODO Add buffering
            //TODO Add processor and system check
            var device = new System.Device.I2c.Drivers.UnixI2cDevice(new System.Device.I2c.I2cConnectionSettings(BusId, address));
            device.Write(data);
        }
    }
}