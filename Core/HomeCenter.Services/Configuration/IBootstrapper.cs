using Proto;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Services.Configuration
{
    public interface IBootstrapper : IDisposable
    {
        Task<PID> BuildController();
    }
}