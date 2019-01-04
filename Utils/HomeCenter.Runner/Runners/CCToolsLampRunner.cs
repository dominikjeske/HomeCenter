using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Events.Device;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    public class CCToolsLampRunner : Runner
    {
        public CCToolsLampRunner(string uid) : base(uid)
        {
            _tasks = new string[] { "TurnOn", "TurnOff", "Refresh", "Test"};
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
                    cmd = new RefreshCommand();
                    cmd.LogLevel = nameof(Microsoft.Extensions.Logging.LogLevel.Information);
                    break;
                case 3:
                    var ev = PropertyChangedEvent.Create("HSPE16InputOnly_1", "PowerState", true, false, new Dictionary<string, string>()
                    {
                        [MessageProperties.PinNumber] = 1.ToString()
                    });
                    MessageBroker.PublishEvent(ev, "HSPE16InputOnly_1");
                    return Task.CompletedTask;
                    //cmd = new SwitchPowerStateCommand();
                    //cmd[MessageProperties.PinNumber] = "5";
                    //break;

            }

            MessageBroker.Send(cmd, Uid);

            return Task.CompletedTask;
        }
    }
}