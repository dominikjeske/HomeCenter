using HomeCenter.Adapters.PC.Messages;
using HomeCenter.Adapters.PC.Model;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Queries.Device;
using Proto;
using System;
using System.Threading.Tasks;

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
            await base.OnStarted(context).ConfigureAwait(false);

            _hostname = AsString(MessageProperties.Hostname);
            _port = AsInt(MessageProperties.Port);
            _mac = AsString(MessageProperties.MAC);
            _poolInterval = AsIntTime(MessageProperties.PoolInterval, DEFAULT_POOL_INTERVAL);

            await ScheduleDeviceRefresh<RefreshStateJob>(_poolInterval).ConfigureAwait(false);
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

            var state = await MessageBroker.QueryJsonService<ComputerQuery, ComputerStatus>(cmd).ConfigureAwait(false);

            _input = await UpdateState(InputSourceState.StateName, _input, state.ActiveInput).ConfigureAwait(false);
            _volume = await UpdateState(VolumeState.StateName, _volume, state.MasterVolume.Value).ConfigureAwait(false);
            _mute = await UpdateState(MuteState.StateName, _mute, state.Mute).ConfigureAwait(false);
            _powerState = await UpdateState(PowerState.StateName, _powerState, state.PowerStatus).ConfigureAwait(false);
        }

        protected async Task Handle(TurnOnCommand message)
        {
            var cmd = WakeOnLanCommand.Create(_mac);
            await MessageBroker.SendToService(cmd).ConfigureAwait(false);

            //TODO check state before update the state
            _powerState = await UpdateState(PowerState.StateName, _powerState, true).ConfigureAwait(false);
        }

        protected async Task Handle(TurnOffCommand message)
        {
            var cmd = new ComputerCommand
            {
                Address = _hostname,
                Service = "Power",
                Message = new PowerPost { State = 0 } //Hibernate
            };
            await MessageBroker.SendToService(cmd).ConfigureAwait(false);
            _powerState = await UpdateState(PowerState.StateName, _powerState, false).ConfigureAwait(false);
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
            await MessageBroker.SendToService(cmd).ConfigureAwait(false);
            _volume = await UpdateState(VolumeState.StateName, _volume, volume).ConfigureAwait(false);
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
            await MessageBroker.SendToService(cmd).ConfigureAwait(false);

            _volume = await UpdateState(VolumeState.StateName, _volume, volume).ConfigureAwait(false);
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
            await MessageBroker.SendToService(cmd).ConfigureAwait(false);

            _volume = await UpdateState(VolumeState.StateName, _volume, volume).ConfigureAwait(false);
        }

        protected async Task Handle(MuteCommand message)
        {
            var cmd = new ComputerCommand
            {
                Address = _hostname,
                Service = "Mute",
                Message = new MutePost { Mute = true }
            };
            await MessageBroker.SendToService(cmd).ConfigureAwait(false);

            _mute = await UpdateState(MuteState.StateName, _mute, true).ConfigureAwait(false);
        }

        protected async Task Handle(UnmuteCommand message)
        {
            var cmd = new ComputerCommand
            {
                Address = _hostname,
                Service = "Mute",
                Message = new MutePost { Mute = false }
            };
            await MessageBroker.SendToService(cmd).ConfigureAwait(false);

            _mute = await UpdateState(MuteState.StateName, _mute, false).ConfigureAwait(false);
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
            await MessageBroker.SendToService(cmd).ConfigureAwait(false);

            _input = await UpdateState(InputSourceState.StateName, _input, inputName).ConfigureAwait(false);
        }
    }
}