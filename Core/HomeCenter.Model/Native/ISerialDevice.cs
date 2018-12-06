using System;
using System.Threading.Tasks;

namespace HomeCenter.Model.Native
{
    public interface ISerialDevice : IDisposable
    {
        void Init();
        void Send(byte[] data);
        void Send(string data);
    }
}