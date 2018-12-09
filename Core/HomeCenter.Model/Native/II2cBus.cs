using System.Threading.Tasks;

namespace HomeCenter.Model.Native
{
    public interface II2cBus
    {
        Task Write(int address, byte[] data);
    }
}