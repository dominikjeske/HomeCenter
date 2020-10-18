using HomeCenter.Model.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;

namespace HomeCenter.Devices
{
    // TODO try use build-in from .net core and https://github.com/dotnet/iot/pull/452

    public class I2cBus : II2cBus
    {
        private const int I2CSlave = 0x0703;
        private const int OpenReadWrite = 0x2;

        private readonly object _accessLock = new object();
        private readonly ILogger<I2cBus> _logger;
        private readonly string _filename;

        private int _handle;
        private bool _isEnabled = false;

        public I2cBus(ILogger<I2cBus> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _filename = "/dev/i2c-" + 1;
        }

        private void CheckEnabled()
        {
            if (_isEnabled) return;

            _handle = NativeOpen(_filename, OpenReadWrite);
            _isEnabled = true;

            if (_handle != 0)
            {
                _logger.Log(LogLevel.Trace, "Opened '{filename}' (Handle = {handle}).", _filename, _handle);
            }
            else
            {
                _logger.Log(LogLevel.Error, "Error while opening '{filename}'.", _filename);
            }
        }

        public void Write(int address, byte[] data)
        {
            lock (_accessLock)
            {
                CheckEnabled();

                var ioCtlResult = NativeIoctl(_handle, I2CSlave, address);
                var writeResult = NativeWrite(_handle, data, data.Length, 0);

                //_logger.Log(LogLevel.Information, $"Written on '{_filename}' (Device address = {address}; Buffer = {data}; IOCTL result = {ioCtlResult}; Write result = {writeResult}; Error = {Marshal.GetLastWin32Error()}).");
            }
        }

        public void Read(int address, byte[] buffer)
        {
            lock (_accessLock)
            {
                CheckEnabled();

                NativeIoctl(_handle, I2CSlave, address);
                NativeRead(_handle, buffer, buffer.Length, 0);
            }
        }

        public void WriteRead(int deviceAddress, byte[] writeBuffer, byte[] readBuffer)
        {
            lock (_accessLock)
            {
                CheckEnabled();

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