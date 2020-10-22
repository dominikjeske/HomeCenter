using HomeCenter.Adapters.PC.Messages;
using HomeCenter.Adapters.PC.Model;
using HomeCenter.CodeGeneration;
using System;
using System.Threading.Tasks;
using HomeCenter.Abstractions;
using HomeCenter.Actors.Core;
using HomeCenter.Capabilities;
using HomeCenter.Messages.Commands.Device;
using HomeCenter.Messages.Commands.Service;
using HomeCenter.Messages.Queries.Device;
using Proto;

namespace HomeCenter.Adapters.PC
{
    [ProxyCodeGenerator]
    public abstract class PcAdapter : Adapter
    {
        private const int DEFAULT_POOL_INTERVAL = 1000;

        private string _hostname;
        private int _port;
        private string _mac;
        private TimeSpan _poolInterval;

        private bool _powerState;
        private double _volume;
        private bool _mute;
        private string _input;

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context);

            _hostname = this.AsString(MessageProperties.Hostname);
            _port = this.AsInt(MessageProperties.Port);
            _mac = this.AsString(MessageProperties.MAC);
            _poolInterval = this.AsIntTime(MessageProperties.PoolInterval, DEFAULT_POOL_INTERVAL);

            await ScheduleDeviceRefresh(_poolInterval);
        }

        protected DiscoveryResponse Discover(DiscoverQuery message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerState(),
                                                               new VolumeState(),
                                                               new MuteState(),
                                                               new InputSourceState()
                                          );
        }

        protected async Task Handle(RefreshCommand message)
        {
            if (!IsEnabled) return;

            var cmd = new ComputerQuery
            {
                Address = _hostname,
                Port = _port,
                Service = "Status"
            };

            var state = await MessageBroker.QueryJsonService<ComputerQuery, ComputerStatus>(cmd);

            _input = await UpdateState(InputSourceState.StateName, _input, state.ActiveInput);
            _volume = await UpdateState(VolumeState.StateName, _volume, state.MasterVolume.Value);
            _mute = await UpdateState(MuteState.StateName, _mute, state.Mute);
            _powerState = await UpdateState(PowerState.StateName, _powerState, state.PowerStatus);
        }

        protected async Task Handle(TurnOnCommand message)
        {
            var cmd = WakeOnLanCommand.Create(_mac);
            await MessageBroker.SendToService(cmd);

            //TODO check state before update the state
            _powerState = await UpdateState(PowerState.StateName, _powerState, true);
        }

        protected async Task Handle(TurnOffCommand message)
        {
            var cmd = new ComputerCommand
            {
                Address = _hostname,
                Service = "Power",
                Message = new PowerPost { State = 0 } //Hibernate
            };
            await MessageBroker.SendToService(cmd);
            _powerState = await UpdateState(PowerState.StateName, _powerState, false);
        }

        protected async Task Handle(VolumeUpCommand command)
        {
            var volume = _volume + command.AsDouble(MessageProperties.ChangeFactor);
            var cmd = new ComputerCommand
            {
                Address = _hostname,
                Service = "Volume",
                Message = new VolumePost { Volume = volume }
            };
            await MessageBroker.SendToService(cmd);
            _volume = await UpdateState(VolumeState.StateName, _volume, volume);
        }

        protected async Task Handle(VolumeDownCommand command)
        {
            var volume = _volume - command.AsDouble(MessageProperties.ChangeFactor);
            var cmd = new ComputerCommand
            {
                Address = _hostname,
                Service = "Volume",
                Message = new VolumePost { Volume = volume }
            };
            await MessageBroker.SendToService(cmd);

            _volume = await UpdateState(VolumeState.StateName, _volume, volume);
        }

        protected async Task Handle(VolumeSetCommand command)
        {
            var volume = command.AsDouble(MessageProperties.Value);
            var cmd = new ComputerCommand
            {
                Address = _hostname,
                Service = "Volume",
                Message = new VolumePost { Volume = volume }
            };
            await MessageBroker.SendToService(cmd);

            _volume = await UpdateState(VolumeState.StateName, _volume, volume);
        }

        protected async Task Handle(MuteCommand message)
        {
            var cmd = new ComputerCommand
            {
                Address = _hostname,
                Service = "Mute",
                Message = new MutePost { Mute = true }
            };
            await MessageBroker.SendToService(cmd);

            _mute = await UpdateState(MuteState.StateName, _mute, true);
        }

        protected async Task Handle(UnmuteCommand message)
        {
            var cmd = new ComputerCommand
            {
                Address = _hostname,
                Service = "Mute",
                Message = new MutePost { Mute = false }
            };
            await MessageBroker.SendToService(cmd);

            _mute = await UpdateState(MuteState.StateName, _mute, false);
        }

        protected async Task Handle(InputSetCommand message)
        {
            var inputName = message.AsString(MessageProperties.InputSource);

            var cmd = new ComputerCommand
            {
                Address = _hostname,
                Service = "InputSource",
                Message = new InputSourcePost { Input = inputName }
            };
            await MessageBroker.SendToService(cmd);

            _input = await UpdateState(InputSourceState.StateName, _input, inputName);
        }
    }
}