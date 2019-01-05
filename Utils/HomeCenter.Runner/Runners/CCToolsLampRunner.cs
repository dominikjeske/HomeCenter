using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Utils.ConsoleExtentions;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    public class CCToolsLampRunner : Runner
    {
        int? pinNumber = 10;

        public CCToolsLampRunner(string uid) : base(uid)
        {
            _tasks = new string[] { "TurnOn", "TurnOff", "Refresh", "Switch"};
        }

        public override void RunnerReset()
        {
            base.RunnerReset();

            pinNumber = 10;
        }

        public override Task RunTask(int taskId)
        {
            if(!pinNumber.HasValue)
            {
                ConsoleEx.WriteWarning("Write PIN number:");
                pinNumber = int.Parse(Console.ReadLine());
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
                    cmd.LogLevel = nameof(Microsoft.Extensions.Logging.LogLevel.Information);
                    break;
                case 3:
                    cmd = new SwitchPowerStateCommand();
                    break;

            }

            if(pinNumber.HasValue && pinNumber.Value < 10)
            {
                cmd.SetProperty(MessageProperties.PinNumber, pinNumber.Value);
            }

            MessageBroker.Send(cmd, Uid);

            return Task.CompletedTask;
        }
    }
}