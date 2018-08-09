using System;
using System.Threading.Tasks;

namespace HomeCenter.Core
{
    public interface IService : IDisposable
    {
        Task Initialize();
    }
}
