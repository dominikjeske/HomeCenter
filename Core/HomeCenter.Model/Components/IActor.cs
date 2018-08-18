using HomeCenter.ComponentModel.Commands;
using HomeCenter.Core;
using System.Threading.Tasks;

namespace HomeCenter.ComponentModel.Components
{
    public interface IActor : IService
    {
        Task ExecuteCommand(Command command);

        Task<T> ExecuteQuery<T>(Command command);

        string Uid { get; }
    }
}