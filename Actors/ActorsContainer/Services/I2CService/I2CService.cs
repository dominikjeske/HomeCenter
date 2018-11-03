using HomeCenter.CodeGeneration;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Queries.Services;
using HomeCenter.Model.Native;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace HomeCenter.Services.Networking
{
    [ProxyCodeGenerator]
    public abstract class I2CService : Service
    {
        private readonly Dictionary<int, II2cDevice> _deviceCache = new Dictionary<int, II2cDevice>();
        private readonly string _busId;
        private readonly II2cBus _nativeI2CBus;

        // Byte 0 = Offset
        // Register 0-1=Input
        // Register 2-3=Output
        // Register 4-5=Inversion
        // Register 6-7=Configuration
        // Register 8=Timeout
        private readonly byte[] _inputWriteBuffer = { 0 };

        protected I2CService(II2cBus nativeI2CBus)
        {
            _nativeI2CBus = nativeI2CBus ?? throw new ArgumentNullException(nameof(nativeI2CBus));
            _busId = _nativeI2CBus.GetBusId();
        }

        private II2cDevice CreateDevice(int slaveAddress) => _nativeI2CBus.CreateDevice(_busId, slaveAddress);

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

        protected void Handle(I2cCommand command)
        {
            var address = I2CSlaveAddress.FromValue(command.Address);
            var useCache = command.UseCache;
            var data = command.Body;

            II2cDevice device = null;
            try
            {
                device = GetDevice(address.Value, useCache);
                var result = device.WritePartial(data);

                if (result.Status != I2cTransferStatus.FullTransfer)
                {
                    Logger.LogWarning($"Transfer failed. Address={address.Value} Status={result.Status} TransferredBytes={result.BytesTransferred}");
                }
            }
            catch (Exception exception)
            {
                Logger.LogWarning(exception, $"Error while accessing I2C device with address {address}.");
            }
            finally
            {
                if (!useCache)
                {
                    device?.Dispose();
                }
            }
        }

        protected byte[] Handle(I2cQuery query)
        {
            var address = I2CSlaveAddress.FromValue(query.Address);
            var useCache = query.UseCache;
            byte[] _inputReadBuffer = new byte[query.Size];
            II2cDevice device = null;

            try
            {
                device = GetDevice(address.Value, useCache);
                var result = device.WriteReadPartial(_inputWriteBuffer, _inputReadBuffer);

                if (result.Status != I2cTransferStatus.FullTransfer)
                {
                    Logger.LogWarning($"Transfer failed. Address={address.Value} Status={result.Status} TransferredBytes={result.BytesTransferred}");
                }
            }
            catch (Exception exception)
            {
                Logger.LogWarning(exception, $"Error while accessing I2C device with address {address}.");
            }
            finally
            {
                if (!useCache)
                {
                    device?.Dispose();
                }
            }

            return _inputReadBuffer;
        }
    }
}