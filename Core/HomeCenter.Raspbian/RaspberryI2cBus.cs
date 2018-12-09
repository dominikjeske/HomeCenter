using HomeCenter.Model.Native;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unosquare.RaspberryIO;

namespace HomeCenter.Raspbian
{
    public class RaspberryI2cBus : II2cBus
    {
        public Task Write(int address, byte[] data)
        {
            var myDevice = Pi.I2C.AddDevice(address);

            return myDevice?.WriteAsync(data);
        }

        public Task<byte[]> Read(int address, int length)
        {
            var myDevice = Pi.I2C.AddDevice(address);
            return myDevice.ReadAsync(length);
        }
    }

    public class LinuxI2CBusAdapter : II2cBus
    {
        private const int I2CSlave = 0x0703;
        private const int OpenReadWrite = 0x2;

        private readonly object _accessLock = new object();
        private readonly string _filename;

        private int _handle;

        public LinuxI2CBusAdapter()
        {
            var busId = 1;
            _filename = "/dev/i2c-" + busId;

            Enable();
        }

        public void Enable()
        {
            lock (_accessLock)
            {
                _handle = NativeOpen(_filename, OpenReadWrite);
            }

            //if (_handle != 0)
            //{
            //    _logger.Log(LogLevel.Trace, $"Opened '{_filename}' (Handle = {_handle}).");
            //}
            //else
            //{
            //    _logger.Log(LogLevel.Error, $"Error while opening '{_filename}'.");
            //}
        }

        public Task Write(int deviceAddress, byte[] buffer)
        {
            lock (_accessLock)
            {
                var ioCtlResult = NativeIoctl(_handle, I2CSlave, deviceAddress);
                var writeResult = NativeWrite(_handle, buffer, buffer.Length, 0);

                //_logger.Log(
                //    LogLevel.Debug,
                //    "Written on '{0}' (Device address = {1}; Buffer = {2}; IOCTL result = {3}; Write result = {4}; Error = {5}).",
                //    _filename,
                //    deviceAddress,
                //    buffer.ToHexString(),
                //    ioCtlResult,
                //    writeResult,
                //    Marshal.GetLastWin32Error());
            }

            return Task.CompletedTask;
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