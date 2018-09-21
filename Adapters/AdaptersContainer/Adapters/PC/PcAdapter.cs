using HomeCenter.CodeGeneration;
using HomeCenter.ComponentModel.Capabilities;
using HomeCenter.ComponentModel.Commands;
using HomeCenter.ComponentModel.Commands.Responses;
using HomeCenter.ComponentModel.ValueTypes;
using HomeCenter.Core.Interface.Messaging;
using HomeCenter.Model.Commands.Specialized;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Queries.Specialized;
using Proto;
using System;
using System.Threading.Tasks;

namespace HomeCenter.ComponentModel.Adapters.Pc
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

        protected PcAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
        }

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            _hostname = this[AdapterProperties.Hostname].AsString();
            _port = this[AdapterProperties.Port].AsInt();
            _mac = this[AdapterProperties.MAC].AsString();
            _poolInterval = GetPropertyValue(AdapterProperties.PoolInterval, new IntValue(DEFAULT_POOL_INTERVAL)).AsIntTimeSpan();

            await ScheduleDeviceRefresh<RefreshStateJob>(_poolInterval);
        }

        protected async Task Refresh(RefreshCommand message)
        {
            var state = await _eventAggregator.QueryAsync<ComputerControlMessage, ComputerStatus>(new ComputerControlMessage
            {
                Address = _hostname,
                Port = _port,
                Service = "Status",
                RequestType = "GET"
            }).ConfigureAwait(false);

            _input = await UpdateState<StringValue>(InputSourceState.StateName, _input, state.ActiveInput).ConfigureAwait(false);
            _volume = await UpdateState<DoubleValue>(VolumeState.StateName, _volume, state.MasterVolume).ConfigureAwait(false);
            _mute = await UpdateState<BooleanValue>(MuteState.StateName, _mute, state.Mute).ConfigureAwait(false);
            _powerState = await UpdateState<BooleanValue>(PowerState.StateName, _powerState, state.PowerStatus).ConfigureAwait(false);
        }

        protected DiscoveryResponse Discover(DiscoverQuery message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerState(),
                                                               new VolumeState(),
                                                               new MuteState(),
                                                               new InputSourceState()
                                          );
        }

        protected async Task TurnOm(TurnOnCommand message)
        {
            await _eventAggregator.QueryAsync<WakeOnLanMessage, string>(new WakeOnLanMessage
            {
                MAC = _mac
            }).ConfigureAwait(false);
            _powerState = await UpdateState(PowerState.StateName, _powerState, new BooleanValue(true)).ConfigureAwait(false);
        }

        protected async Task TurnOf(TurnOffCommand message)
        {
            await _eventAggregator.QueryAsync<ComputerControlMessage, string>(new ComputerControlMessage
            {
                Address = _hostname,
                Service = "Power",
                Message = new PowerPost { State = 0 } //Hibernate
            }).ConfigureAwait(false);
            _powerState = await UpdateState(PowerState.StateName, _powerState, new BooleanValue(false)).ConfigureAwait(false);
        }

        protected async Task VolumeUp(VolumeUpCommand command)
        {
            var volume = _volume + command[CommandProperties.ChangeFactor].AsDouble();

            await _eventAggregator.QueryAsync<ComputerControlMessage, string>(new ComputerControlMessage
            {
                Address = _hostname,
                Service = "Volume",
                Message = new VolumePost { Volume = volume }
            }).ConfigureAwait(false);

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume)).ConfigureAwait(false);
        }

        protected async Task VolumeDown(VolumeDownCommand command)
        {
            var volume = _volume - command[CommandProperties.ChangeFactor].AsDouble();
            await _eventAggregator.QueryAsync<ComputerControlMessage, string>(new ComputerControlMessage
            {
                Address = _hostname,
                Service = "Volume",
                Message = new VolumePost { Volume = volume }
            }).ConfigureAwait(false);

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume)).ConfigureAwait(false);
        }

        protected async Task VolumeSet(VolumeSetCommand command)
        {
            var volume = command[CommandProperties.Value].AsDouble();
            await _eventAggregator.QueryAsync<ComputerControlMessage, string>(new ComputerControlMessage
            {
                Address = _hostname,
                Service = "Volume",
                Message = new VolumePost { Volume = volume }
            }).ConfigureAwait(false);

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume)).ConfigureAwait(false);
        }

        protected async Task Mute(MuteCommand message)
        {
            await _eventAggregator.QueryAsync<ComputerControlMessage, string>(new ComputerControlMessage
            {
                Address = _hostname,
                Service = "Mute",
                Message = new MutePost { Mute = true }
            }).ConfigureAwait(false);

            _mute = await UpdateState(MuteState.StateName, _mute, new BooleanValue(true)).ConfigureAwait(false);
        }

        protected async Task UnMute(UnmuteCommand message)
        {
            await _eventAggregator.QueryAsync<ComputerControlMessage, string>(new ComputerControlMessage
            {
                Address = _hostname,
                Service = "Mute",
                Message = new MutePost { Mute = false }
            }).ConfigureAwait(false);

            _mute = await UpdateState(MuteState.StateName, _mute, new BooleanValue(false)).ConfigureAwait(false);
        }

        protected async Task InputSet(InputSetCommand message)
        {
            var inputName = (StringValue)message[CommandProperties.InputSource];

            await _eventAggregator.QueryAsync<ComputerControlMessage, string>(new ComputerControlMessage
            {
                Address = _hostname,
                Service = "InputSource",
                Message = new InputSourcePost { Input = inputName }
            }).ConfigureAwait(false);

            _input = await UpdateState(InputSourceState.StateName, _input, inputName).ConfigureAwait(false);
        }
    }
}