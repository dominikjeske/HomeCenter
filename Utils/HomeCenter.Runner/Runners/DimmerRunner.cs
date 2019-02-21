using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Events.Device;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Collections.Generic;
using System.Threading.Tasks;
using HomeCenter.Model.Extensions;
using System;

namespace HomeCenter.Runner
{
    public class DimmerRunner : Runner
    {
        public DimmerRunner(string uid) : base(uid)
        {
            _tasks = new string[] { "TurnOn", "TurnOff", "PropertyChanged", "REL_ON", "REL_OFF", "Timmer" };
        }

        public override async Task RunTask(int taskId)
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
                    var ev = PropertyChangedEvent.Create("DimmerBridge", "CurrentState", "0.0", "0.170631870627403", new Dictionary<string, string> {[MessageProperties.PinNumber] = "0" });
                    MessageBroker.Send(ev, "Dimmer");
                    return;
                case 3:
                    cmd = new TurnOnCommand();
                    cmd.SetProperty(MessageProperties.PinNumber, 0);
                    MessageBroker.Send(cmd, "HSRel8_1");
                    return;
                case 4:
                    cmd = new TurnOffCommand();
                    cmd.SetProperty(MessageProperties.PinNumber, 0);
                    MessageBroker.Send(cmd, "HSRel8_1");
                    return;
                    break;
                case 5:

                    var scheduler = Container.GetInstance<IScheduler>();
                    var logger = Container.GetInstance<ILogger<object>>();

                    logger.LogWarning("Schedule");
                    await scheduler.DelayExecution<TestJob, ILogger>(TimeSpan.FromMilliseconds(3000), logger, "uid");

                    return;
                    break;
            }

            

            MessageBroker.Send(cmd, Uid);

            return;
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