using System;
using System.Threading.Tasks;

namespace HomeCenter.Core.Interface.Native
{
    public interface ISerialDevice : IDisposable
    {
        Task Init();
        IBinaryReader GetBinaryReader();
    }
}