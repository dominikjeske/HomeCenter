using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Commands.Device;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    public class DimmerRunner : Runner
    {
        public DimmerRunner(string uid) : base(uid)
        {
            _tasks = new string[] { "TurnOn", "TurnOff" };
        }

        public override Task RunTask(int taskId)
        {
            Command cmd = null;
            switch (taskId)
            {
                case 0:
                    cmd = new TurnOnCommand();
                    break;

                case 1:
                    cmd = new TurnOffCommand();
                    break;
            }

            MessageBroker.Send(cmd, Uid);

            return Task.CompletedTask;
        }
    }
}