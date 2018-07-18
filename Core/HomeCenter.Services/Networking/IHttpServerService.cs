using HTTPnet.Core.Pipeline;
using HomeCenter.Core;

namespace HomeCenter.Services.Networking
{
    public interface IHttpServerService : IService
    {
        void AddRequestHandler(IHttpContextPipelineHandler handler);

        void UpdateServerPort(int port);
    }
}