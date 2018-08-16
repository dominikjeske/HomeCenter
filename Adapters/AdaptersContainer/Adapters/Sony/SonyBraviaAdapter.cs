using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HomeCenter.ComponentModel.Capabilities;
using HomeCenter.ComponentModel.Commands;
using HomeCenter.ComponentModel.Commands.Responses;
using HomeCenter.ComponentModel.ValueTypes;
using HomeCenter.Core.Extensions;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Extensions;

namespace HomeCenter.ComponentModel.Adapters.Sony
{
    // TODO test when power off
    public class SonyBraviaAdapter : Adapter
    {
        private const int DEFAULT_POOL_INTERVAL = 1000;

        private BooleanValue _powerState;
        private DoubleValue _volume;
        private BooleanValue _mute;
        private StringValue _input;

        private TimeSpan _poolInterval;
        private string _hostname;
        private string _authorisationKey;

        private Dictionary<string, string> _inputSourceMap = new Dictionary<string, string>
        {
            { "HDMI1", "AAAAAgAAABoAAABaAw==" },
            { "HDMI2", "AAAAAgAAABoAAABbAw==" },
            { "HDMI3", "AAAAAgAAABoAAABcAw==" },
            { "HDMI4", "AAAAAgAAABoAAABdAw==" }
        };

        public SonyBraviaAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
        }

        public override Task Initialize()
        {
            if (!IsEnabled) return Task.CompletedTask;
            base.Initialize();

            _hostname = this[AdapterProperties.Hostname].AsString();
            _authorisationKey = this[AdapterProperties.AuthKey].AsString();
            _poolInterval = GetPropertyValue(AdapterProperties.PoolInterval, new IntValue(DEFAULT_POOL_INTERVAL)).AsIntTimeSpan();

            return ScheduleDeviceRefresh<RefreshStateJob>(_poolInterval);
        }

        protected async Task RefreshCommandHandler(Command message)
        {
            var power = await _eventAggregator.QueryAsync<SonyJsonMessage, string>(new SonyJsonMessage
            {
                Address = _hostname,
                AuthorisationKey = _authorisationKey,
                Path = "system",
                Method = "getPowerStatus"
            }).ConfigureAwait(false);

            //TODO
            //_powerState = await UpdateState<BooleanValue>(PowerState.StateName, _powerState, power);

            var audio = await _eventAggregator.QueryAsync<SonyJsonMessage, string>(new SonyJsonMessage
            {
                Address = _hostname,
                AuthorisationKey = _authorisationKey,
                Path = "audio",
                Method = "getVolumeInformation"
            }).ConfigureAwait(false);
        }

        protected DiscoveryResponse DiscoverCapabilitiesHandler(Command message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerState(),
                                                               new VolumeState(),
                                                               new MuteState(),
                                                               new InputSourceState()
                                          );
        }

        protected async Task TurnOnCommandHandler(Command message)
        {
            var result = await _eventAggregator.QueryAsync<SonyControlMessage, string>(new SonyControlMessage
            {
                Address = _hostname,
                AuthorisationKey = _authorisationKey,
                Code = "AAAAAQAAAAEAAAAuAw=="
            }).ConfigureAwait(false);
            _powerState = await UpdateState(PowerState.StateName, _powerState, new BooleanValue(true)).ConfigureAwait(false);
        }

        protected async Task TurnOffCommandHandler(Command message)
        {
            var result = await _eventAggregator.QueryAsync<SonyControlMessage, string>(new SonyControlMessage
            {
                Address = _hostname,
                AuthorisationKey = _authorisationKey,
                Code = "AAAAAQAAAAEAAAAvAw=="
            }).ConfigureAwait(false);
            _powerState = await UpdateState(PowerState.StateName, _powerState, new BooleanValue(false)).ConfigureAwait(false);
        }

        protected async Task VolumeUpCommandHandler(Command command)
        {
            var volume = _volume + command[CommandProperties.ChangeFactor].AsDouble();
            await _eventAggregator.QueryAsync<SonyJsonMessage, string>(new SonyJsonMessage
            {
                Address = _hostname,
                AuthorisationKey = _authorisationKey,
                Path = "audio",
                Method = "setAudioVolume",
                Params = new SonyAudioVolumeRequest("speaker", ((int)volume).ToString())
            }).ConfigureAwait(false);

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume)).ConfigureAwait(false);
        }

        protected async Task VolumeDownCommandHandler(Command command)
        {
            var volume = _volume - command[CommandProperties.ChangeFactor].AsDouble();
            await _eventAggregator.QueryAsync<SonyJsonMessage, string>(new SonyJsonMessage
            {
                Address = _hostname,
                AuthorisationKey = _authorisationKey,
                Path = "audio",
                Method = "setAudioVolume",
                Params = new SonyAudioVolumeRequest("speaker", ((int)volume).ToString())
            }).ConfigureAwait(false);

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume)).ConfigureAwait(false);
        }

        protected async Task VolumeSetCommandHandler(Command command)
        {
            var volume = command[CommandProperties.Value].AsDouble();
            await _eventAggregator.QueryAsync<SonyJsonMessage, string>(new SonyJsonMessage
            {
                Address = _hostname,
                AuthorisationKey = _authorisationKey,
                Path = "audio",
                Method = "setAudioVolume",
                Params = new SonyAudioVolumeRequest("speaker", ((int)volume).ToString())
            }).ConfigureAwait(false);

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume)).ConfigureAwait(false);
        }

        protected async Task MuteCommandHandler(Command message)
        {
            await _eventAggregator.QueryAsync<SonyJsonMessage, string>(new SonyJsonMessage
            {
                Address = _hostname,
                AuthorisationKey = _authorisationKey,
                Path = "audio",
                Method = "setAudioMute",
                Params = new SonyAudioMuteRequest(true)
            }).ConfigureAwait(false);

            _mute = await UpdateState(MuteState.StateName, _mute, new BooleanValue(true)).ConfigureAwait(false);
        }

        protected async Task UnmuteCommandHandler(Command message)
        {
            await _eventAggregator.QueryAsync<SonyJsonMessage, string>(new SonyJsonMessage
            {
                Address = _hostname,
                AuthorisationKey = _authorisationKey,
                Path = "audio",
                Method = "setAudioMute",
                Params = new SonyAudioMuteRequest(false)
            }).ConfigureAwait(false);

            _mute = await UpdateState(MuteState.StateName, _mute, new BooleanValue(false)).ConfigureAwait(false);
        }

        protected async Task SelectInputCommandHandler(Command message)
        {
            var inputName = (StringValue)message[CommandProperties.InputSource];
            if (!_inputSourceMap.ContainsKey(inputName)) throw new UnsupportedPropertyStateException($"Input {inputName} was not found on available device input sources");

            var cmd = _inputSourceMap[inputName];

            var result = await _eventAggregator.QueryAsync<SonyControlMessage, string>(new SonyControlMessage
            {
                Address = _hostname,
                AuthorisationKey = _authorisationKey,
                Code = cmd
            }).ConfigureAwait(false);
            _input = await UpdateState(InputSourceState.StateName, _input, inputName).ConfigureAwait(false);
        }
    }
}