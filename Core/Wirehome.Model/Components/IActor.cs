using System.Threading.Tasks;
using Wirehome.ComponentModel.Commands;

namespace Wirehome.ComponentModel.Components
{
    public interface IActor
    {
        Task<object> ExecuteCommand(Command command);
        Task<T> ExecuteCommand<T>(Command command);
    }
}