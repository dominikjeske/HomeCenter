using HomeCenter.Adapters.Sony.Messages;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Runner.ConsoleExtentions;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    public class SonyRunner : Runner
    {
        public SonyRunner(string uid) : base(uid)
        {
            _tasks = new string[] { "VolumeUp", "VolumeDown", "TurnOn", "TurnOff", "VolumeSet", "Mute", "Unmute", "InputSet", "Register" };
        }

        public override async Task RunTask(int taskId)
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
                    MessageBroker.Send(InputSetCommand.Create("HDMI1"), Uid);
                    break;

                case 8:

                    var command = new SonyRegisterQuery();
                    var result = await MessageBroker.Request<SonyRegisterQuery, string>(command, "Sony");

                    ConsoleEx.WriteTitleLine("Enter PIN from TV:");
                    var pin = Console.ReadLine();
                    command.PIN = pin;
                    result = await MessageBroker.Request<SonyRegisterQuery, string>(command, "Sony");

                    Console.WriteLine($"Device was registered successfully. Application hash: {result}");
                    break;
            }
        }
    }
}