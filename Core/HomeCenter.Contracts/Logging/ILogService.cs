
namespace HomeCenter.Core.Services.Logging
{
    public interface ILogService : IService
    {
        ILogger CreatePublisher(string source);
    }
}
