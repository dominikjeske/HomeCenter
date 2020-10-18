using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    public interface IRunner
    {
        Task Run(int taskId);
    }
}