using HTTPnet.Core.Pipeline;

namespace HomeCenter.Services
{
    public interface IHttpServerService
    {
        void AddRequestHandler(IHttpContextPipelineHandler handler);

        void UpdateServerPort(int port);
    }
}