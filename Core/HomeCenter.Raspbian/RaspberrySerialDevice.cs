using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Native;
using System;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace HomeCenter.Raspbian
{
    public class RaspberrySerialDevice : ISerialDevice
    {
        private SerialPort _serialPort;
        private Subject<byte[]> _dataSink;

        public IObservable<byte[]> DataSink => _dataSink.AsObservable();


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

            _dataSink = new Subject<byte[]>();
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
            byte[] buffer = new byte[_serialPort.BytesToRead];
            _serialPort.Read(buffer, 0, _serialPort.BytesToRead);

            _dataSink.OnNext(buffer);
        }
    }
}