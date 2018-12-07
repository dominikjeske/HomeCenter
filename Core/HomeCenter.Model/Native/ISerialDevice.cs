using System;

namespace HomeCenter.Model.Native
{
    public interface ISerialDevice : IDisposable
    {
        IObservable<byte[]> DataSink { get; }

        void Init();

        void Send(byte[] data);

        void Send(string data);
    }
}