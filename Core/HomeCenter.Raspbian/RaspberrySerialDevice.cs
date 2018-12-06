using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Native;
using System;
using System.IO;
using System.IO.Ports;
using System.Linq;

namespace HomeCenter.Raspbian
{
    public class RaspberrySerialDevice : ISerialDevice
    {
        private SerialPort _serialPort;

        public void Dispose()
        {
            if (_serialPort != null)
            {
                _serialPort.DataReceived -= _serialPort_DataReceived;
                _serialPort.Close();
            }
        }

        public void Init()
        {
            _serialPort = new SerialPort();

            var portName = SerialPort.GetPortNames().FirstOrDefault();

            if (string.IsNullOrWhiteSpace(portName)) throw new InitializationException("COM port was not found on RaspberryPI");

            _serialPort.PortName = portName;
            _serialPort.BaudRate = 115200;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;

            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            _serialPort.DataReceived += _serialPort_DataReceived;

            _serialPort.Open();
        }

        public void Send(byte[] data)
        {
            _serialPort.Write(data, 0, data.Length);
        }

        public void Send(string data)
        {
            _serialPort.Write(data);
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Initialize a buffer to hold the received data
            byte[] buffer = new byte[_serialPort.ReadBufferSize];

            Console.WriteLine($"Buffer size {_serialPort.ReadBufferSize}");
            Console.WriteLine($"Bytes to read {_serialPort.BytesToRead}");

            // There is no accurate method for checking how many bytes are read
            // unless you check the return from the Read method
            int bytesRead = _serialPort.Read(buffer, 0, buffer.Length);

            Console.WriteLine($"Bytes read {bytesRead}");

            //byte[] buffer2 = new byte[_serialPort.BytesToRead];
            //_serialPort.Read(buffer2, 0, _serialPort.BytesToRead);

            using (var stream = new MemoryStream(buffer))
            {
                var reader = new BinaryReader(stream);
            }
        }
    }
}