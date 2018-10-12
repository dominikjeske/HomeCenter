using System.Threading.Tasks;

namespace HomeCenter.TestRunner
{
    public class DenonRunner : Runner
    {
        public DenonRunner() : base(new string[] { "VolumeUp", "VolumeDown" })
        {
        }

        public override Task RunTask(int taskId)
        {
            return Task.CompletedTask;
        }
    }
}