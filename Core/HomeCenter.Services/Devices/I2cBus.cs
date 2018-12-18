using HomeCenter.Model.Contracts;
using System;
using System.Collections.Generic;
using System.Device.I2c;

namespace HomeCenter.Services.Devices
{
    public class I2cBus : II2cBus, IDisposable
    {
        private const int BusId = 1;
        private readonly Dictionary<int, I2cDevice> _deviceCache = new Dictionary<int, I2cDevice>();

        public void Write(int address, Span<byte> data)
        {
            CheckCache(address);

            _deviceCache[address].Write(data);
        }

        public Span<byte> Read(int address)
        {
            CheckCache(address);

            var buffer = new Span<byte>();
            _deviceCache[address].Read(buffer);

            Console.WriteLine($"Read from {address} -> {buffer.Length}");

            return buffer;
        }

        private void CheckCache(int address)
        {
            if (!_deviceCache.ContainsKey(address))
            {
                _deviceCache[address] = new System.Device.I2c.Drivers.UnixI2cDevice(new I2cConnectionSettings(BusId, address));
            }
        }

        public void Dispose()
        {
            foreach (var device in _deviceCache.Values)
            {
                device.Dispose();
            }
        }
    }
}