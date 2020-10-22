using HomeCenter.CodeGeneration;
using System;

namespace HomeCenter.Services.Networking
{
    [ProxyCodeGenerator]
    public abstract class I2CService : Service
    {
        private readonly II2cBus _nativeI2CBus;

        protected I2CService(II2cBus nativeI2CBus)
        {
            _nativeI2CBus = nativeI2CBus;
        }

        [Subscribe]
        protected void Handle(I2cCommand command)
        {
            var address = command.Address;
            var data = command.Body;

            CheckAddress(address);

            try
            {
                _nativeI2CBus.Write(address, data);
            }
            catch (Exception exception)
            {
                Logger.LogWarning(exception, "Error while accessing I2C device with address {address}. {exception}", address, exception.Message);
            }
        }

        [Subscribe]
        protected byte[] Handle(I2cQuery query)
        {
            var address = query.Address;
            var bufferSize = query.BufferSize;
            byte[] initializeWrite = Array.Empty<byte>();

            if (query.ContainsProperty(MessageProperties.Initialize))
            {
                initializeWrite = query.Initialize;
            }

            try
            {
                var result = new byte[bufferSize];

                if (initializeWrite != Array.Empty<byte>())
                {
                    _nativeI2CBus.WriteRead(address, initializeWrite, result);
                }
                else
                {
                    _nativeI2CBus.Read(address, result);
                }

                return result;
            }
            catch (Exception exception)
            {
                Logger.LogWarning(exception, "Error while accessing I2C device with address {address}. {exception}", address, exception.Message);
                return Array.Empty<byte>();
            }
        }

        private void CheckAddress(int value)
        {
            if (value < 0 || value > 127) throw new ArgumentOutOfRangeException(nameof(value), "I2C address is invalid.");
            if (value >= 0x00 && value <= 0x07) throw new ArgumentOutOfRangeException(nameof(value), "I2C address " + value + " is reserved.");
            if (value >= 0x78 && value <= 0x7f) throw new ArgumentOutOfRangeException(nameof(value), "I2C address " + value + " is reserved.");
        }
    }
}