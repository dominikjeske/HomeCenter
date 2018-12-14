using System;

namespace HomeCenter.Model.Contracts
{
    public interface II2cBus
    {
        void Write(int address, Span<byte> data);
    }
}