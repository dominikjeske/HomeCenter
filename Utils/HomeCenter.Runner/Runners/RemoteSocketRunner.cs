using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Services.Devices;
using HomeCenter.Services.Networking;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{

    public class RemoteSocketRunner : Runner
    {
        GpioDevice gpioService = new GpioDevice();

        public RemoteSocketRunner(string uid) : base(uid, new string[] { "TurnOn", "TurnOff", "TurnOn LED", "TurnOff LED" })
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
                case 2:
                    gpioService.Write(16, false);

                    break;
                case 3:
                    gpioService.Write(16, true);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}