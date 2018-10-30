using HomeCenter.Model.Components;
using HomeCenter.Model.Messages.Commands.Device;
using System.Threading.Tasks;

namespace HomeCenter.TestRunner
{
    public class DenonRunner : Runner
    {
        public DenonRunner(string uid) : base(uid, new string[] { "VolumeUp", "VolumeDown" })
        {
        }

        public override Task RunTask(int taskId)
        {
            switch (taskId)
            {
                case 0:
                    MessageBroker.Send(VolumeUpCommand.Default, Uid);
                    break;
                case 1:
                    MessageBroker.Send(VolumeDownCommand.Default, Uid);
                    break;
            }

            return Task.CompletedTask;
        }

        
    }
}