using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HomeCenter.Core.Interface.Native;
using HomeCenter.Core.Services.I2C;
using Microsoft.Extensions.Logging;

namespace HomeCenter.Core.Services
{
    public sealed class I2CBusService : II2CBusService
    {
        private readonly Dictionary<int, II2cDevice> _deviceCache = new Dictionary<int, II2cDevice>();
        private readonly string _busId;
        private readonly II2cBus _nativeI2CBus;
        private readonly ILogger<I2CBusService> _logger;

        public I2CBusService(ILogger<I2CBusService> logger, II2cBus nativeI2CBus)
        {
            _nativeI2CBus = nativeI2CBus ?? throw new ArgumentNullException(nameof(nativeI2CBus));
            _busId = _nativeI2CBus.GetBusId();
            _logger = logger;
        }

        private II2cDevice CreateDevice(int slaveAddress)
        {
            return _nativeI2CBus.CreateDevice(_busId, slaveAddress);
        }

        public I2cTransferResult Write(I2CSlaveAddress address, byte[] buffer, bool useCache = true)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            return Execute(address, d => d.WritePartial(buffer), useCache);
        }

        public I2cTransferResult Read(I2CSlaveAddress address, byte[] buffer, bool useCache = true)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            return Execute(address, d => d.ReadPartial(buffer), useCache);
        }

        public I2cTransferResult WriteRead(I2CSlaveAddress address, byte[] writeBuffer, byte[] readBuffer, bool useCache = true)
        {
            if (writeBuffer == null) throw new ArgumentNullException(nameof(writeBuffer));
            if (readBuffer == null) throw new ArgumentNullException(nameof(readBuffer));

            return Execute(address, d => d.WriteReadPartial(writeBuffer, readBuffer), useCache);
        }

        private I2cTransferResult Execute(I2CSlaveAddress address, Func<II2cDevice, I2cTransferResult> action, bool useCache = true)
        {
            lock (_deviceCache)
            {
                II2cDevice device = null;
                try
                {
                    device = GetDevice(address.Value, useCache);
                    var result = action(device);

                    if (result.Status != I2cTransferStatus.FullTransfer)
                    {
                        _logger.LogWarning($"Transfer failed. Address={address.Value} Status={result.Status} TransferredBytes={result.BytesTransferred}");
                    }

                    return WrapResult(result);
                }
                catch (Exception exception)
                {
                    // Ensure that the application will not crash if some devices are currently not available etc.
                    _logger.LogWarning(exception, $"Error while accessing I2C device with address {address}.");
                    return new I2cTransferResult() { Status = I2cTransferStatus.UnknownError, BytesTransferred = 0 };
                }
                finally
                {
                    if (!useCache)
                    {
                        device?.Dispose();
                    }
                }
            }
        }

        private static I2cTransferResult WrapResult(I2cTransferResult result)
        {
            var status = I2cTransferStatus.UnknownError;
            switch (result.Status)
            {
                case I2cTransferStatus.FullTransfer:
                    {
                        status = I2cTransferStatus.FullTransfer;
                        break;
                    }

                case I2cTransferStatus.PartialTransfer:
                    {
                        status = I2cTransferStatus.PartialTransfer;
                        break;
                    }

                case I2cTransferStatus.ClockStretchTimeout:
                    {
                        status = I2cTransferStatus.ClockStretchTimeout;
                        break;
                    }

                case I2cTransferStatus.SlaveAddressNotAcknowledged:
                    {
                        status = I2cTransferStatus.SlaveAddressNotAcknowledged;
                        break;
                    }
            }

            return new I2cTransferResult() { Status = status, BytesTransferred = result.BytesTransferred };
        }

        private II2cDevice GetDevice(int address, bool useCache)
        {
            // The Arduino Nano T&H bridge does not work correctly when reusing the device. More investigation is required!
            // At this time, the cache can be disabled for certain devices.
            if (!useCache)
            {
                return CreateDevice(address);
            }

            if (!_deviceCache.TryGetValue(address, out II2cDevice device))
            {
                device = CreateDevice(address);
                _deviceCache.Add(address, device);
            }

            return device;
        }

        public Task Initialize()
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            
        }
    }
}