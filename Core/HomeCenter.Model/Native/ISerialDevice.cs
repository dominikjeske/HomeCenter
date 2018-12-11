using System;

namespace HomeCenter.Model.Devices
{
    public interface ISerialDevice : IDisposable
    {
        IObservable<byte[]> DataSink { get; }

        void Init();

        void Send(byte[] data);

        void Send(string data);
    }
}