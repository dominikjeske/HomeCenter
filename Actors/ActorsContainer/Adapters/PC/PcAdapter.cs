using HomeCenter.Adapters.PC.Messages;
using HomeCenter.Adapters.PC.Model;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Model.ValueTypes;
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

        private BooleanValue _powerState;
        private DoubleValue _volume;
        private BooleanValue _mute;
        private StringValue _input;

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            _hostname = this[AdapterProperties.Hostname].AsString();
            _port = this[AdapterProperties.Port].AsInt();
            _mac = this[AdapterProperties.MAC].AsString();
            _poolInterval = GetPropertyValue(AdapterProperties.PoolInterval, new IntValue(DEFAULT_POOL_INTERVAL)).AsIntTimeSpan();

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

            _input = await UpdateState<StringValue>(InputSourceState.StateName, _input, state.ActiveInput).ConfigureAwait(false);
            _volume = await UpdateState<DoubleValue>(VolumeState.StateName, _volume, state.MasterVolume).ConfigureAwait(false);
            _mute = await UpdateState<BooleanValue>(MuteState.StateName, _mute, state.Mute).ConfigureAwait(false);
            _powerState = await UpdateState<BooleanValue>(PowerState.StateName, _powerState, state.PowerStatus).ConfigureAwait(false);
        }
        
        protected async Task Handle(TurnOnCommand message)
        {
            var cmd = new WakeOnLanCommand(_mac);
            await MessageBroker.SendToService(cmd).ConfigureAwait(false);

            //TODO check state before update the state
            _powerState = await UpdateState(PowerState.StateName, _powerState, new BooleanValue(true)).ConfigureAwait(false);
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
            _powerState = await UpdateState(PowerState.StateName, _powerState, new BooleanValue(false)).ConfigureAwait(false);
        }

        protected async Task Handle(VolumeUpCommand command)
        {
            var volume = _volume + command[CommandProperties.ChangeFactor].AsDouble();
            var cmd = new ComputerCommand
            {
                Address = _hostname,
                Service = "Volume",
                Message = new VolumePost { Volume = volume }
            };
            await MessageBroker.SendToService(cmd).ConfigureAwait(false);
            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume)).ConfigureAwait(false);
        }

        protected async Task Handle(VolumeDownCommand command)
        {
            var volume = _volume - command[CommandProperties.ChangeFactor].AsDouble();
            var cmd = new ComputerCommand
            {
                Address = _hostname,
                Service = "Volume",
                Message = new VolumePost { Volume = volume }
            };
            await MessageBroker.SendToService(cmd).ConfigureAwait(false);

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume)).ConfigureAwait(false);
        }

        protected async Task Handle(VolumeSetCommand command)
        {
            var volume = command[CommandProperties.Value].AsDouble();
            var cmd = new ComputerCommand
            {
                Address = _hostname,
                Service = "Volume",
                Message = new VolumePost { Volume = volume }
            };
            await MessageBroker.SendToService(cmd).ConfigureAwait(false);

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume)).ConfigureAwait(false);
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
 
            _mute = await UpdateState(MuteState.StateName, _mute, new BooleanValue(true)).ConfigureAwait(false);
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
 
            _mute = await UpdateState(MuteState.StateName, _mute, new BooleanValue(false)).ConfigureAwait(false);
        }

        protected async Task Handle(InputSetCommand message)
        {
            var inputName = (StringValue)message[CommandProperties.InputSource];

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