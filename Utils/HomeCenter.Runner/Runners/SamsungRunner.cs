using HomeCenter.Model.Messages.Commands.Device;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    public class SamsungRunner : Runner
    {
        public SamsungRunner(string uid, string address) : base(uid, address, new string[] { "VolumeUp", "VolumeDown", "TurnOn", "TurnOff",  "Mute",  "InputSelect" })
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

                case 2:
                    MessageBroker.Send(TurnOnCommand.Default, Uid);
                    break;

                case 3:
                    MessageBroker.Send(TurnOffCommand.Default, Uid);
                    break;

                case 4:
                    MessageBroker.Send(MuteCommand.Default, Uid);
                    break;

                case 5:
                    MessageBroker.Send(InputSetCommand.Create("HDMI"), Uid);
                    break;


            }

            return Task.CompletedTask;
        }
    }
}