using System;
using System.Threading.Tasks;

namespace HomeCenter.Model.Core
{
    public interface IService : IDisposable
    {
        Task Initialize();
    }
}
