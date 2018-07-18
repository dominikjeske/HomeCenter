using System.Threading.Tasks;
using HomeCenter.ComponentModel.Commands;

namespace HomeCenter.ComponentModel.Components
{
    public interface IActor
    {
        Task<object> ExecuteCommand(Command command);
        Task<T> ExecuteCommand<T>(Command command);
    }
}