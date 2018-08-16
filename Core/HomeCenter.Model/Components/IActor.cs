using HomeCenter.ComponentModel.Commands;
using HomeCenter.Core;
using System.Threading.Tasks;

namespace HomeCenter.ComponentModel.Components
{
    public interface IActor : IService
    {
        Task<object> ExecuteCommand(Command command);

        Task<T> ExecuteCommand<T>(Command command);
    }
}