using System;

namespace HomeCenter.Model.Contracts
{
    public interface II2cBus : IDisposable
    {
        void Write(int address, Span<byte> data);
        Span<byte> Read(int address);
    }
}