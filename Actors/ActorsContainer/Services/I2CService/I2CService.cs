using HomeCenter.CodeGeneration;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Contracts;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Queries.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;

namespace HomeCenter.Services.Networking
{
    [ProxyCodeGenerator]
    public abstract class I2CService : Service
    {
        private readonly II2cBus _nativeI2CBus;

        private LinuxI2CBusAdapter _ada;

        protected I2CService(II2cBus nativeI2CBus)
        {
            _nativeI2CBus = nativeI2CBus;

            _disposables.Add(_nativeI2CBus);
        }

        [Subscibe]
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
                Logger.LogWarning(exception, $"Error while accessing I2C device with address {address}. {exception.Message}");
            }
        }

        [Subscibe]
        protected byte[] Handle(I2cQuery query)
        {
            var address = query.Address;
            byte[] initializeWrite = null;

            if (query.ContainsProperty(MessageProperties.Initialize))
            {
                initializeWrite = query.Initialize;
            }
            
            try
            {

                if (_ada == null)
                {
                    _ada = new LinuxI2CBusAdapter(Logger);
                    _ada.Enable();
                }
                var readBuffer = new byte[2];
                _ada.WriteRead(address, initializeWrite, readBuffer);
                return readBuffer;

                //if (initializeWrite != null)
                //{
                //  _nativeI2CBus.Write(address, initializeWrite);
                //}

                // return _nativeI2CBus.Read(address).ToArray();
            }
            catch (Exception exception)
            {
                Logger.LogWarning(exception, $"Error while accessing I2C device with address {address}. {exception.Message}");
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


    public class LinuxI2CBusAdapter
    {
        private const int I2CSlave = 0x0703;
        private const int OpenReadWrite = 0x2;

        private readonly object _accessLock = new object();
        private readonly ILogger _logger;
        private readonly string _filename;

        private int _handle;

        public LinuxI2CBusAdapter(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _filename = "/dev/i2c-" + 1;
        }

        public void Enable()
        {
            lock (_accessLock)
            {
                _handle = NativeOpen(_filename, OpenReadWrite);
            }

            if (_handle != 0)
            {
                _logger.Log(LogLevel.Trace, $"Opened '{_filename}' (Handle = {_handle}).");
            }
            else
            {
                _logger.Log(LogLevel.Error, $"Error while opening '{_filename}'.");
            }
        }

        public void Write(int deviceAddress, byte[] buffer)
        {
            lock (_accessLock)
            {
                var ioCtlResult = NativeIoctl(_handle, I2CSlave, deviceAddress);
                var writeResult = NativeWrite(_handle, buffer, buffer.Length, 0);

                _logger.Log(LogLevel.Information, $"Written on '{_filename}' (Device address = {deviceAddress}; Buffer = {buffer}; IOCTL result = {ioCtlResult}; Write result = {writeResult}; Error = {Marshal.GetLastWin32Error()}).");
            }
        }

        public void Read(int deviceAddress, byte[] buffer)
        {
            lock (_accessLock)
            {
                NativeIoctl(_handle, I2CSlave, deviceAddress);
                NativeRead(_handle, buffer, buffer.Length, 0);
            }
        }

        public void WriteRead(int deviceAddress, byte[] writeBuffer, byte[] readBuffer)
        {
            lock (_accessLock)
            {
                NativeIoctl(_handle, I2CSlave, deviceAddress);
                NativeWrite(_handle, writeBuffer, writeBuffer.Length, 0);
                NativeRead(_handle, readBuffer, readBuffer.Length, 0);
            }
        }

        [DllImport("libc.so.6", EntryPoint = "open", SetLastError = true)]
        private static extern int NativeOpen(string fileName, int mode);

        [DllImport("libc.so.6", EntryPoint = "ioctl", SetLastError = true)]
        private static extern int NativeIoctl(int fd, int request, int data);

        [DllImport("libc.so.6", EntryPoint = "read", SetLastError = true)]
        private static extern int NativeRead(int handle, byte[] data, int length, int offset);

        [DllImport("libc.so.6", EntryPoint = "write", SetLastError = true)]
        private static extern int NativeWrite(int handle, byte[] data, int length, int offset);
    }


}