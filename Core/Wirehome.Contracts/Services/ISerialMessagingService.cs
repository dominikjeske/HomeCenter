using System;
using System.Threading.Tasks;
using Wirehome.Core.Interface.Messaging;
using Wirehome.Core.Interface.Native;

namespace Wirehome.Core.Services
{
    public interface ISerialMessagingService : IService
    {
        void RegisterMessageHandler(Func<byte, byte, IBinaryReader, Task<bool>> handler);
    }
}