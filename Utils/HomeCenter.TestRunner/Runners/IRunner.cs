using System.Threading.Tasks;

namespace HomeCenter.TestRunner
{
    public interface IRunner
    {
        Task Run(int taskId);
    }
}