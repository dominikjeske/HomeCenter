using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Wirehome.Core.Interface.Native;

namespace Wirehome.Raspberry
{
    internal class RaspberrySerialDevice : INativeSerialDevice
    {
        private SerialDevice _serialDevice;

        public void Dispose() => _serialDevice?.Dispose();
        public IBinaryReader GetBinaryReader() => new BinaryReader(_serialDevice.InputStream);

        public async Task Init()
        {
            string aqs = SerialDevice.GetDeviceSelector("UART0");                   /* Find the selector string for the serial device   */
            var dis = await DeviceInformation.FindAllAsync(aqs);                    /* Find the serial device with our selector string  */
            _serialDevice = await SerialDevice.FromIdAsync(dis[0].Id);    /* Create an serial device with our selected device */
            
            //var devices = await DeviceInformation.FindAllAsync(SerialDevice.GetDeviceSelector());
            //var firstDevice = devices.FirstOrDefault();
            //_serialDevice = await SerialDevice.FromIdAsync(firstDevice.Id);
       
            if (_serialDevice == null) throw new Exception("UART port not found on device");

            _serialDevice.WriteTimeout = TimeSpan.FromMilliseconds(1000);
            _serialDevice.ReadTimeout = TimeSpan.FromMilliseconds(1000);
            _serialDevice.BaudRate = 115200;
            _serialDevice.Parity = SerialParity.None;
            _serialDevice.StopBits = SerialStopBitCount.One;
            _serialDevice.DataBits = 8;
            _serialDevice.Handshake = SerialHandshake.None;
        }
    }
}
