using System;

namespace HomeCenter.Model.Native
{
    public interface II2cBus
    {
        void Write(int address, Span<byte> data);
    }
}