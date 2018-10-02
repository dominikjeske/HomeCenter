using System;
using System.Threading.Tasks;

namespace HomeCenter.Model.Native
{
    public interface ISerialDevice : IDisposable
    {
        Task Init();

        IBinaryReader GetBinaryReader();
    }
}