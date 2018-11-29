using HomeCenter.Model.Messages.Commands.Device;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    public class KodiRunner : Runner
    {
        public KodiRunner(string uid) : base(uid, new string[] { "VolumeUp", "VolumeDown", "TurnOn", "TurnOff", "VolumeSet", "Mute", "Unmute", "Play", "Stop" })
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
                    MessageBroker.Send(VolumeSetCommand.Create(40), Uid);
                    break;

                case 5:
                    MessageBroker.Send(MuteCommand.Default, Uid);
                    break;

                case 6:
                    MessageBroker.Send(UnmuteCommand.Default, Uid);
                    break;

                case 7:
                    MessageBroker.Send(PlayCommand.Default, Uid);
                    break;

                case 8:
                    MessageBroker.Send(StopCommand.Default, Uid);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}