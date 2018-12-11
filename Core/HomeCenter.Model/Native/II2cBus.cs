using System;

namespace HomeCenter.Model.Devices
{
    public interface II2cBus
    {
        void Write(int address, Span<byte> data);
    }
}