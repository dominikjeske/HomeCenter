using HomeCenter.Messages;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Utils.ConsoleExtentions;
using HomeCenter.Utils.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    public class DenonRunner : Runner
    {
        public DenonRunner(string uid) : base(uid, new string[] { "VolumeUp", "VolumeDown", "TurnOn", "TurnOff", "VolumeSet", "Mute", "Unmute", "InputSet", "ModeSet", "Capabilities", "States", "Tags", "Status", "Test" })
        {
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
                    ConsoleEx.WriteOKLine($"Capabilities: {capabilities.ToFormatedString()}");
                    break;
                case 10:
                    var states = await MessageBroker.Request<SupportedStatesQuery, IReadOnlyCollection<string>>(SupportedStatesQuery.Default, Uid);
                    ConsoleEx.WriteOKLine($"Supported States: {states.ToFormatedString()}");
                    break;
                case 11:
                    var tags = await MessageBroker.Request<TagsQuery, IReadOnlyCollection<string>>(TagsQuery.Default, Uid);
                    ConsoleEx.WriteOKLine($"Tags: {tags.ToFormatedString()}");
                    break;
                case 12:
                    var status = await MessageBroker.Request<StatusQuery, IReadOnlyCollection<State>>(StatusQuery.Default, Uid);
                    ConsoleEx.WriteOKLine($"Status: {status.ToFormatedString(" | ")}");
                    break;
                case 13:
                    var command = new ProtoCommand { Type = "TurnOnCommand" };
                    command.Properties.Add("XXX", "YYY");
                    MessageBroker.Send(command, Uid);
                    break;

            }

            return;
        }
    }
}