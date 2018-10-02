using HomeCenter.Adapters.Sony.Messages;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Model.ValueTypes;
using Proto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.Sony
{
    // TODO test when power off
    [ProxyCodeGenerator]
    public abstract class SonyBraviaAdapter : Adapter
    {
        private const int DEFAULT_POOL_INTERVAL = 1000;

        private BooleanValue _powerState;
        private DoubleValue _volume;
        private BooleanValue _mute;
        private StringValue _input;

        private TimeSpan _poolInterval;
        private string _hostname;
        private string _authorisationKey;

        private readonly Dictionary<string, string> _inputSourceMap = new Dictionary<string, string>
        {
            { "HDMI1", "AAAAAgAAABoAAABaAw==" },
            { "HDMI2", "AAAAAgAAABoAAABbAw==" },
            { "HDMI3", "AAAAAgAAABoAAABcAw==" },
            { "HDMI4", "AAAAAgAAABoAAABdAw==" }
        };

        protected SonyBraviaAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
        }

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            _hostname = this[AdapterProperties.Hostname].AsString();
            _authorisationKey = this[AdapterProperties.AuthKey].AsString();
            _poolInterval = GetPropertyValue(AdapterProperties.PoolInterval, new IntValue(DEFAULT_POOL_INTERVAL)).AsIntTimeSpan();

            await ScheduleDeviceRefresh<RefreshStateJob>(_poolInterval);
        }

        protected async Task Refresh(RefreshCommand message)
        {
            var power = await _eventAggregator.QueryAsync<SonyJsonCommand, string>(new SonyJsonCommand
            {
                Address = _hostname,
                AuthorisationKey = _authorisationKey,
                Path = "system",
                Method = "getPowerStatus"
            }).ConfigureAwait(false);

            //TODO
            //_powerState = await UpdateState<BooleanValue>(PowerState.StateName, _powerState, power);

            var audio = await _eventAggregator.QueryAsync<SonyJsonCommand, string>(new SonyJsonCommand
            {
                Address = _hostname,
                AuthorisationKey = _authorisationKey,
                Path = "audio",
                Method = "getVolumeInformation"
            }).ConfigureAwait(false);
        }

        protected DiscoveryResponse Discover(DiscoverQuery message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerState(),
                                                               new VolumeState(),
                                                               new MuteState(),
                                                               new InputSourceState()
                                          );
        }

        protected async Task TurnOn(TurnOnCommand message)
        {
            var result = await _eventAggregator.QueryAsync<SonyControlCommand, string>(new SonyControlCommand
            {
                Address = _hostname,
                AuthorisationKey = _authorisationKey,
                Code = "AAAAAQAAAAEAAAAuAw=="
            }).ConfigureAwait(false);
            _powerState = await UpdateState(PowerState.StateName, _powerState, new BooleanValue(true)).ConfigureAwait(false);
        }

        protected async Task TurnOff(TurnOffCommand message)
        {
            var result = await _eventAggregator.QueryAsync<SonyControlCommand, string>(new SonyControlCommand
            {
                Address = _hostname,
                AuthorisationKey = _authorisationKey,
                Code = "AAAAAQAAAAEAAAAvAw=="
            }).ConfigureAwait(false);
            _powerState = await UpdateState(PowerState.StateName, _powerState, new BooleanValue(false)).ConfigureAwait(false);
        }

        protected async Task VolumeUp(VolumeUpCommand command)
        {
            var volume = _volume + command[CommandProperties.ChangeFactor].AsDouble();
            await _eventAggregator.QueryAsync<SonyJsonCommand, string>(new SonyJsonCommand
            {
                Address = _hostname,
                AuthorisationKey = _authorisationKey,
                Path = "audio",
                Method = "setAudioVolume",
                Params = new SonyAudioVolumeRequest("speaker", ((int)volume).ToString())
            }).ConfigureAwait(false);

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume)).ConfigureAwait(false);
        }

        protected async Task VolumeDown(VolumeDownCommand command)
        {
            var volume = _volume - command[CommandProperties.ChangeFactor].AsDouble();
            await _eventAggregator.QueryAsync<SonyJsonCommand, string>(new SonyJsonCommand
            {
                Address = _hostname,
                AuthorisationKey = _authorisationKey,
                Path = "audio",
                Method = "setAudioVolume",
                Params = new SonyAudioVolumeRequest("speaker", ((int)volume).ToString())
            }).ConfigureAwait(false);

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume)).ConfigureAwait(false);
        }

        protected async Task VolumeSer(VolumeSetCommand command)
        {
            var volume = command[CommandProperties.Value].AsDouble();
            await _eventAggregator.QueryAsync<SonyJsonCommand, string>(new SonyJsonCommand
            {
                Address = _hostname,
                AuthorisationKey = _authorisationKey,
                Path = "audio",
                Method = "setAudioVolume",
                Params = new SonyAudioVolumeRequest("speaker", ((int)volume).ToString())
            }).ConfigureAwait(false);

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume)).ConfigureAwait(false);
        }

        protected async Task Mute(MuteCommand message)
        {
            await _eventAggregator.QueryAsync<SonyJsonCommand, string>(new SonyJsonCommand
            {
                Address = _hostname,
                AuthorisationKey = _authorisationKey,
                Path = "audio",
                Method = "setAudioMute",
                Params = new SonyAudioMuteRequest(true)
            }).ConfigureAwait(false);

            _mute = await UpdateState(MuteState.StateName, _mute, new BooleanValue(true)).ConfigureAwait(false);
        }

        protected async Task UnMute(UnmuteCommand message)
        {
            await _eventAggregator.QueryAsync<SonyJsonCommand, string>(new SonyJsonCommand
            {
                Address = _hostname,
                AuthorisationKey = _authorisationKey,
                Path = "audio",
                Method = "setAudioMute",
                Params = new SonyAudioMuteRequest(false)
            }).ConfigureAwait(false);

            _mute = await UpdateState(MuteState.StateName, _mute, new BooleanValue(false)).ConfigureAwait(false);
        }

        protected async Task InputSet(InputSetCommand message)
        {
            var inputName = (StringValue)message[CommandProperties.InputSource];
            if (!_inputSourceMap.ContainsKey(inputName)) throw new UnsupportedPropertyStateException($"Input {inputName} was not found on available device input sources");

            var cmd = _inputSourceMap[inputName];

            var result = await _eventAggregator.QueryAsync<SonyControlCommand, string>(new SonyControlCommand
            {
                Address = _hostname,
                AuthorisationKey = _authorisationKey,
                Code = cmd
            }).ConfigureAwait(false);
            _input = await UpdateState(InputSourceState.StateName, _input, inputName).ConfigureAwait(false);
        }
    }
}