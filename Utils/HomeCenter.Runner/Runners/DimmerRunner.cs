using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Utils.ConsoleExtentions;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    public class DimmerRunner : Runner
    {
        public DimmerRunner(string uid) : base(uid)
        {
            _tasks = new string[] { "TurnOn", "TurnOff", "SetDimmerLevel" };
        }

        public override Task RunTask(int taskId)
        {
            Command cmd = null;
            switch (taskId)
            {
                case 0:
                    cmd = new TurnOnCommand();

                    ConsoleEx.WriteTitleLine("Time:");
                    var time = ConsoleEx.ReadNumber();
                    cmd.SetProperty(MessageProperties.StateTime, time);

                    break;

                case 1:
                    //cmd = new TurnOffCommand();
                    cmd = new TurnOnCommand();

                    cmd.SetProperty(MessageProperties.StateTime, 100);

                    break;

                case 2:
                    ConsoleEx.WriteTitleLine("Enter dimmer level [0-100]:");
                    var level = ConsoleEx.ReadNumber();
                    cmd = new SetPowerLevelCommand();
                    ((SetPowerLevelCommand)cmd).PowerLevel = level;
                    break;
            }

            MessageBroker.Send(cmd, Uid);

            return Task.CompletedTask;
        }
    }

    public class TestJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var logger = context.GetDataContext<ILogger>();
            logger.LogWarning("Executed");

            return Task.CompletedTask;
        }
    }
}