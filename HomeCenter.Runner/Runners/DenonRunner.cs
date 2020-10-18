using HomeCenter.Extensions;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Runner.ConsoleExtentions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    public class DenonRunner : Runner
    {
        public DenonRunner(string uid) : base(uid)
        {
            _tasks = new string[] { "VolumeUp", "VolumeDown", "TurnOn", "TurnOff", "VolumeSet", "Mute", "Unmute", "InputSet", "ModeSet", "SupportedStates", "SupportedCapabilities", "State" };
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
                    MessageBroker.Send(InputSetCommand.Create("DVD"), Uid);
                    break;

                case 8:
                    MessageBroker.Send(ModeSetCommand.Create("Movie"), Uid);
                    break;

                case 9:
                    var capabilities = await MessageBroker.Request<CapabilitiesQuery, IReadOnlyCollection<string>>(CapabilitiesQuery.Default, Uid);
                    ConsoleEx.WriteTitleLine($"Capabilities of '{Uid}': {string.Join(", ", capabilities)}");
                    break;

                case 10:
                    var supportedStates = await MessageBroker.Request<SupportedStatesQuery, IReadOnlyCollection<string>>(SupportedStatesQuery.Default, Uid);
                    ConsoleEx.WriteTitleLine($"Supported states of '{Uid}': {string.Join(", ", supportedStates)}");
                    break;

                case 11:
                    var states = await MessageBroker.Request<StateQuery, IReadOnlyDictionary<string, string>>(StateQuery.Default, Uid);
                    ConsoleEx.WriteTitleLine($"State of '{Uid}': {string.Join(", ", states.ToFormatedString())}");
                    break;
            }

            return;
        }
    }
}