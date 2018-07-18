using System;
using System.Threading.Tasks;

namespace Wirehome.Core.Interface.Native
{
    public interface INativeSerialDevice : IDisposable
    {
        Task Init();
        IBinaryReader GetBinaryReader();
    }
}