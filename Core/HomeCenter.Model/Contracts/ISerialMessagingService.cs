using HomeCenter.Core.Interface.Native;
using HomeCenter.Model.Core;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Core.Services
{
    public interface ISerialMessagingService : IService
    {
        void RegisterMessageHandler(Func<byte, byte, IBinaryReader, Task<bool>> handler);
    }
}