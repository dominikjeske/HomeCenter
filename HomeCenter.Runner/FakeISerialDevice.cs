using HomeCenter.Model.Contracts;
using System;
using System.Reactive.Disposables;
using HomeCenter.Abstractions;

namespace HomeCenter.Runner
{

    public class FakeISerialDevice : ISerialDevice
    {
        public void Dispose()
        {
        }

        public void Init()
        {
        }

        public void Send(byte[] data)
        {
        }

        public void Send(string data)
        {
        }

        public IDisposable Subscribe(Action<byte[]> handler) => Disposable.Empty;
    }
}