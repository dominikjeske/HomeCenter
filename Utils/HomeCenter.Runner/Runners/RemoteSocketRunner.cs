using HomeCenter.Model.Messages.Commands.Device;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{

    public class RemoteSocketRunner : Runner
    {
        public RemoteSocketRunner(string uid) : base(uid, new string[] { "TurnOn", "TurnOff"})
        {
        }

        public override Task RunTask(int taskId)
        {
            switch (taskId)
            {
                case 0:
                    MessageBroker.Send(TurnOnCommand.Default, Uid);
                    break;

                case 1:
                    MessageBroker.Send(TurnOffCommand.Default, Uid);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}