using HomeCenter.Abstractions;
using HomeCenter.Messages.Commands.Device;
using HomeCenter.Runner.ConsoleExtentions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    public class DimmerRunner : Runner
    {
        public DimmerRunner(string uid) : base(uid)
        {
            _tasks = new string[] { "TurnOn", "TurnOff", "Turn On (with time)", "SetDimmerLevel", "AdjustPowerLevel" };
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

                case 2:
                    cmd = new TurnOnCommand();

                    ConsoleEx.WriteTitleLine("State Time:");
                    var time = ConsoleEx.ReadNumber();
                    cmd.SetProperty(MessageProperties.StateTime, time);

                    break;

                case 3:
                    ConsoleEx.WriteTitleLine("Enter dimmer level [0-100]:");
                    var level = ConsoleEx.ReadNumber();
                    cmd = new SetPowerLevelCommand();
                    ((SetPowerLevelCommand)cmd).PowerLevel = level;
                    break;

                case 4:
                    ConsoleEx.WriteTitleLine("Adjust Power Level [0-100]:");
                    var delta = ConsoleEx.ReadNumber();
                    cmd = new AdjustPowerLevelCommand();
                    ((AdjustPowerLevelCommand)cmd).Delta = delta;
                    break;
            }

            MessageBroker.Send(cmd, Uid);

            return Task.CompletedTask;
        }
    }
}