using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Utils.ConsoleExtentions;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    public class HSRel8Runner : Runner
    {
        private int? _relayId;

        public HSRel8Runner(string uid) : base(uid)
        {
            _tasks = new string[] { "TurnOn", "TurnOff", "Refresh" };
        }

        public override void RunnerReset()
        {
            _relayId = null;

            base.RunnerReset();
        }

        public override Task RunTask(int taskId)
        {
            if (_relayId == null)
            {
                ConsoleEx.WriteTitleLine("Enter relay number");
                _relayId = ConsoleEx.ReadNumber();
            }

            Command cmd = null;
            switch (taskId)
            {
                case 0:
                    cmd = new TurnOnCommand();
                    break;

                case 1:
                    cmd = new TurnOffCommand();
                    break;
                case 2:
                    cmd = new RefreshCommand();
                    cmd.DefaultLogLevel = Microsoft.Extensions.Logging.LogLevel.Information;
                    break;
            }

            if (cmd != null)
            {
                cmd.SetProperty(MessageProperties.PinNumber, _relayId.Value);
                MessageBroker.Send(cmd, Uid);
            }

            return Task.CompletedTask;
        }
    }
}