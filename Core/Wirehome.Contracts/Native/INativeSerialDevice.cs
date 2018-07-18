using System;
using System.Threading.Tasks;

namespace HomeCenter.Core.Interface.Native
{
    public interface INativeSerialDevice : IDisposable
    {
        Task Init();
        IBinaryReader GetBinaryReader();
    }
}