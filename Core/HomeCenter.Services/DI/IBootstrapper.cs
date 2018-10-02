using Proto;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Services.DI
{
    public interface IBootstrapper : IDisposable
    {
        Task<PID> BuildController();
    }
}