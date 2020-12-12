using HomeCenter.Abstractions;
using HomeCenter.Abstractions.Defaults;
using HomeCenter.Actors.Core;
using HomeCenter.Adapters.Samsung.Messages;
using HomeCenter.Capabilities;
using HomeCenter.Messages.Commands.Device;
using HomeCenter.Messages.Queries.Device;
using Proto;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.Samsung
{
    [Proxy]
    public class SamsungAdapter : Adapter
    {
        private const uint TURN_ON_IR_CODE = 3772793023;
        private string? _hostname;
        private bool _powerState;
        private string? _input;
        private string? _infraredAdaperName;
        private string? _mac;
        private string? _appKey;

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context);

            _hostname = this.AsString(MessageProperties.Hostname);
            _infraredAdaperName = this.AsString(MessageProperties.InfraredAdapter);
            _mac = this.AsString(MessageProperties.MAC);
            _appKey = this.AsString(MessageProperties.AppKey, "HomeCenter");
        }

        protected DiscoveryResponse Discover(DiscoverQuery message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerState(ReadWriteMode.Write),
                                                               new VolumeState(ReadWriteMode.Write),
                                                               new MuteState(ReadWriteMode.Write),
                                                               new InputSourceState(ReadWriteMode.Write)
                                          );
        }

        private SamsungControlCommand GetCommand(string code)
        {
            if (_hostname is null) throw new InvalidOperationException();
            if (_mac is null) throw new InvalidOperationException();
            if (_appKey is null) throw new InvalidOperationException();

            return new SamsungControlCommand
            {
                Address = _hostname,
                Code = code,
                MAC = _mac,
                AppKey = _appKey
            };
        }

        protected Task Handle(TurnOnCommand message)
        {
            if (_infraredAdaperName is null) throw new InvalidOperationException();

            MessageBroker.Send(SendCodeCommand.Create(TURN_ON_IR_CODE), _infraredAdaperName);
            return Task.CompletedTask;
        }

        protected async Task Handle(TurnOffCommand message)
        {
            var cmd = GetCommand("KEY_POWEROFF");
            await MessageBroker.SendToService(cmd);

            _powerState = await UpdateState(PowerState.StateName, _powerState, false);
        }

        protected Task Handle(VolumeUpCommand command)
        {
            var cmd = GetCommand("KEY_VOLUP");
            return MessageBroker.SendToService(cmd);
        }

        protected Task Handle(VolumeDownCommand command)
        {
            var cmd = GetCommand("KEY_VOLDOWN");
            return MessageBroker.SendToService(cmd);
        }

        protected Task Handle(MuteCommand message)
        {
            var cmd = GetCommand("KEY_MUTE");
            return MessageBroker.SendToService(cmd);
        }

        protected async Task Handle(InputSetCommand message)
        {
            var inputName = message.AsString(MessageProperties.InputSource);

            var source = "";
            if (inputName == "HDMI")
            {
                source = "KEY_HDMI";
            }
            else if (inputName == "AV")
            {
                source = "KEY_AV1";
            }
            else if (inputName == "COMPONENT")
            {
                source = "KEY_COMPONENT1";
            }
            else if (inputName == "TV")
            {
                source = "KEY_TV";
            }

            if (source?.Length == 0) throw new ArgumentException($"Input {inputName} was not found on Samsung available device input sources");
            if (source is null) throw new InvalidOperationException();

            var cmd = GetCommand(source);
            await MessageBroker.SendToService(cmd);

            _input = await UpdateState(InputSourceState.StateName, _input, inputName);
        }
    }
}