using Microsoft.Extensions.Logging;
using System;


namespace HomeCenter
{
    public class I2CService 
    {
        private readonly II2cBus _nativeI2CBus;
        private readonly ILogger _logger;

        public I2CService(II2cBus nativeI2CBus, ILogger logger)
        {
            _nativeI2CBus = nativeI2CBus;
            _logger = logger;
        }

        public void Send(int address, byte[] data)
        {
            CheckAddress(address);

            try
            {
                _nativeI2CBus.Write(address, data);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Error while accessing I2C device with address {address}. {exception}", address, exception.Message);
            }
        }

        public byte[] Get(int address, int bufferSize, byte[] initializeWrite)
        {
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
                _logger.LogWarning(exception, "Error while accessing I2C device with address {address}. {exception}", address, exception.Message);
                return Array.Empty<byte>();
            }
        }

        private void CheckAddress(int value)
        {
            if (value < 0 || value > 127)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "I2C address is invalid.");
            }

            if (value >= 0x00 && value <= 0x07)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "I2C address " + value + " is reserved.");
            }

            if (value >= 0x78 && value <= 0x7f)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "I2C address " + value + " is reserved.");
            }
        }
    }
}