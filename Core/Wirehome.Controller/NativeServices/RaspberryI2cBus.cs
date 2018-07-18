using System;
using System.Linq;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Wirehome.Core.Interface.Native;

namespace Wirehome.Raspberry
{
    internal class RaspberryI2cBus : INativeI2cBus
    {
        public INativeI2cDevice CreateDevice(string deviceId, int slaveAddress)
        {
            var settings = new I2cConnectionSettings(slaveAddress)
            {
                BusSpeed = I2cBusSpeed.StandardMode,
                SharingMode = I2cSharingMode.Exclusive
            };

            var device = I2cDevice.FromIdAsync(deviceId, settings).GetAwaiter().GetResult();

            if (device == null) throw new Exception($"Device {deviceId} was not found on I2C bus");

            return new RaspberryI2cDevice(device);
        }

        public string GetBusId()
        {
            var deviceSelector = I2cDevice.GetDeviceSelector();
            var deviceInformation = DeviceInformation.FindAllAsync(deviceSelector).GetAwaiter().GetResult();

            if (deviceInformation.Count == 0)
            {
                // TODO: Allow local controller to replace this. Then throw exception again
                throw new InvalidOperationException("I2C bus not found.");
            }

            return deviceInformation.First().Id;
        }
    }
}
