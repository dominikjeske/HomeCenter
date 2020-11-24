using HomeCenter.Abstractions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace HomeCenter.Devices
{
    public class SerialDevice : ISerialDevice
    {
        private SerialPort? _serialPort;
        private readonly Subject<byte[]> _dataSink = new Subject<byte[]>();
        private readonly object _locki = new object();
        private bool _isInitialized;

        public void Dispose()
        {
            if (_serialPort != null)
            {
                _serialPort.DataReceived -= _serialPort_DataReceived;
                _serialPort.Close();
            }
        }

        [MemberNotNull(nameof(_serialPort))]
        private void TryInit()
        {
#pragma warning disable CS8774 // Member must have a non-null value when exiting.
            if (_isInitialized) return;
#pragma warning restore CS8774 // Member must have a non-null value when exiting.

            lock (_locki)
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

                _isInitialized = true;
            }
        }

        public void Send(byte[] data)
        {
            TryInit();

            _serialPort.Write(data, 0, data.Length);
        }

        public void Send(string data)
        {
            TryInit();

            _serialPort.Write(data);
        }

        public IDisposable Subscribe(Action<byte[]> handler) => _dataSink.Subscribe(handler);

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            TryInit();

            byte[] buffer = new byte[_serialPort.BytesToRead];
            _serialPort.Read(buffer, 0, _serialPort.BytesToRead);

            _dataSink.OnNext(buffer);
        }
    }
}