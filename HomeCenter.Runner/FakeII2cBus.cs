using HomeCenter.Abstractions;
using HomeCenter.Model.Contracts;

namespace HomeCenter.Runner
{

    public class FakeII2cBus : II2cBus
    {
        public void Read(int address, byte[] buffer)
        {
        }

        public void Write(int address, byte[] data)
        {
        }

        public void WriteRead(int deviceAddress, byte[] writeBuffer, byte[] readBuffer)
        {
        }
    }
}