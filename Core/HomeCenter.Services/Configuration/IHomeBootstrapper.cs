using System;
using System.Threading.Tasks;
using HomeCenter.Model.Core;
using SimpleInjector;

namespace HomeCenter.Services.Configuration
{
    public interface IBootstrapper : IDisposable
    {
        Task<Controller> BuildController();
    }
}