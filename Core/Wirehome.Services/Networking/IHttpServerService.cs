using HTTPnet.Core.Pipeline;
using Wirehome.Core;

namespace Wirehome.Services.Networking
{
    public interface IHttpServerService : IService
    {
        void AddRequestHandler(IHttpContextPipelineHandler handler);

        void UpdateServerPort(int port);
    }
}