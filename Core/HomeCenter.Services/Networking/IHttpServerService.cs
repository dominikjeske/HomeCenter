using HomeCenter.Model.Core;
using HTTPnet.Core.Pipeline;

namespace HomeCenter.Services.Networking
{
    public interface IHttpServerService : IService
    {
        void AddRequestHandler(IHttpContextPipelineHandler handler);

        void UpdateServerPort(int port);
    }
}