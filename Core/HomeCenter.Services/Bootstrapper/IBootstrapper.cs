using Proto;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Services.Bootstrapper
{
    public interface IBootstrapper : IDisposable
    {
        Task<PID> BuildController();
    }
}