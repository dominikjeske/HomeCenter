using System;
using System.Threading.Tasks;
using HomeCenter.Core.Interface.Messaging;
using HomeCenter.Core.Interface.Native;

namespace HomeCenter.Core.Services
{
    public interface ISerialMessagingService : IService
    {
        void RegisterMessageHandler(Func<byte, byte, IBinaryReader, Task<bool>> handler);
    }
}