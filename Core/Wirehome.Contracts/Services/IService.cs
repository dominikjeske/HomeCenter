using System;
using System.Threading.Tasks;

namespace Wirehome.Core
{
    public interface IService : IDisposable
    {
        Task Initialize();
    }
}
