using System;
using System.Threading.Tasks;
using HomeCenter.ComponentModel.Capabilities;
using HomeCenter.ComponentModel.Commands;
using HomeCenter.ComponentModel.Commands.Responses;
using HomeCenter.ComponentModel.ValueTypes;
using HomeCenter.Core.Extensions;
using HomeCenter.Model.Extensions;

namespace HomeCenter.ComponentModel.Adapters.Denon
{
    public class DenonAdapter : Adapter
    {
        public const int DEFAULT_POOL_INTERVAL = 1000;

        private BooleanValue _powerState;
        private DoubleValue _volume;
        private BooleanValue _mute;
        private StringValue _input;
        private StringValue _surround;
        private DenonDeviceInfo _fullState;
        private string _hostName;
        private int _zone;
        private TimeSpan _poolInterval;

        public DenonAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
        }

        public override async Task Initialize()
        {
            await base.Initialize().ConfigureAwait(false);

            _hostName = this[AdapterProperties.Hostname].AsString();
            _poolInterval = GetPropertyValue(AdapterProperties.PoolInterval, new IntValue(DEFAULT_POOL_INTERVAL)).AsIntTimeSpan();
            //TODO make zone as required parameter
            _zone = this[AdapterProperties.Zone].AsInt();

            if (!IsEnabled) return;

            await ScheduleDeviceRefresh<RefreshLightStateJob>(_poolInterval).ConfigureAwait(false);
            await ExecuteCommand(CommandFatory.RefreshCommand).ConfigureAwait(false);
        }

        protected async Task RefreshCommandHandler(Command message)
        {
            _fullState = await _eventAggregator.QueryAsync<DenonStatusMessage, DenonDeviceInfo>(new DenonStatusMessage { Address = _hostName }).ConfigureAwait(false);
            var mapping = await _eventAggregator.QueryAsync<DenonMappingMessage, DenonDeviceInfo>(new DenonMappingMessage { Address = _hostName }).ConfigureAwait(false);
            _fullState.FriendlyName = mapping.FriendlyName;
            _fullState.InputMap = mapping.InputMap;
            _surround = _fullState.Surround;
        }

        protected async Task RefreshLightCommandHandler(Command message)
        {
            var state = await _eventAggregator.QueryAsync<DenonStatusLightMessage, DenonStatus>(new DenonStatusLightMessage
            {
                Address = _hostName,
                Zone = _zone.ToString()
            }).ConfigureAwait(false);

            _input = await UpdateState<StringValue>(InputSourceState.StateName, _input, state.ActiveInput).ConfigureAwait(false);
            _volume = await UpdateState<DoubleValue>(VolumeState.StateName, _volume, state.MasterVolume).ConfigureAwait(false);
            _mute = await UpdateState<BooleanValue>(MuteState.StateName, _mute, state.Mute).ConfigureAwait(false);
            _powerState = await UpdateState<BooleanValue>(PowerState.StateName, _powerState, state.PowerStatus).ConfigureAwait(false);
        }

        protected DiscoveryResponse DiscoverCapabilitiesHandler(Command message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerState(),
                                                               new VolumeState(),
                                                               new MuteState(),
                                                               new InputSourceState(),
                                                               new SurroundSoundState()
                                          );
        }

        protected async Task TurnOnCommandHandler(Command message)
        {
            await _eventAggregator.QueryWithResultCheckAsync(new DenonControlMessage
            {
                Command = "PowerOn",
                Api = "formiPhoneAppPower",
                ReturnNode = "Power",
                Address = _hostName,
                Zone = _zone.ToString()
            }, "ON").ConfigureAwait(false);
            _powerState = await UpdateState(PowerState.StateName, _powerState, new BooleanValue(true)).ConfigureAwait(false);
        }

        protected async Task TurnOffCommandHandler(Command message)
        {
            await _eventAggregator.QueryWithResultCheckAsync(new DenonControlMessage
            {
                Command = "PowerStandby",
                Api = "formiPhoneAppPower",
                ReturnNode = "Power",
                Address = _hostName,
                Zone = _zone.ToString()
            }, "OFF").ConfigureAwait(false);
            _powerState = await UpdateState(PowerState.StateName, _powerState, new BooleanValue(false)).ConfigureAwait(false);
        }

        protected async Task VolumeUpCommandHandler(Command command)
        {
            var volume = _volume + command[CommandProperties.ChangeFactor].AsDouble();
            var normalized = NormalizeVolume(volume);

            // Results are unpredictyble so we ignore them
            await _eventAggregator.QueryAsync<DenonControlMessage, string>(new DenonControlMessage
            {
                Command = normalized,
                Api = "formiPhoneAppVolume",
                ReturnNode = "MasterVolume",
                Address = _hostName,
                Zone = _zone.ToString()
            }).ConfigureAwait(false);

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume)).ConfigureAwait(false);
        }

        protected async Task VolumeDownCommandHandler(Command command)
        {
            var volume = _volume - command[CommandProperties.ChangeFactor].AsDouble();
            var normalized = NormalizeVolume(volume);

            await _eventAggregator.QueryAsync<DenonControlMessage, string>(new DenonControlMessage
            {
                Command = normalized,
                Api = "formiPhoneAppVolume",
                ReturnNode = "MasterVolume",
                Address = _hostName,
                Zone = _zone.ToString()
            }).ConfigureAwait(false);

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume)).ConfigureAwait(false);
        }

        protected async Task VolumeSetCommandHandler(Command command)
        {
            var volume = command[CommandProperties.Value].AsDouble();
            var normalized = NormalizeVolume(volume);

            await _eventAggregator.QueryAsync<DenonControlMessage, string>(new DenonControlMessage
            {
                Command = normalized,
                Api = "formiPhoneAppVolume",
                ReturnNode = "MasterVolume",
                Address = _hostName,
                Zone = _zone.ToString()
            }).ConfigureAwait(false);

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume)).ConfigureAwait(false);
        }

        private string NormalizeVolume(double volume)
        {
            if (volume < 0) volume = 0;
            if (volume > 100) volume = 100;

            return (volume - 80).ToFloatString();
        }

        protected async Task MuteCommandHandler(Command message)
        {
            await _eventAggregator.QueryWithResultCheckAsync(new DenonControlMessage
            {
                Command = "MuteOn",
                Api = "formiPhoneAppMute",
                ReturnNode = "Mute",
                Address = _hostName,
                Zone = _zone.ToString()
            }, "on").ConfigureAwait(false);

            _mute = await UpdateState(MuteState.StateName, _mute, new BooleanValue(true)).ConfigureAwait(false);
        }

        protected async Task UnmuteCommandHandler(Command message)
        {
            await _eventAggregator.QueryWithResultCheckAsync(new DenonControlMessage
            {
                Command = "MuteOff",
                Api = "formiPhoneAppMute",
                ReturnNode = "Mute",
                Address = _hostName,
                Zone = _zone.ToString()
            }, "off").ConfigureAwait(false);

            _mute = await UpdateState(MuteState.StateName, _mute, new BooleanValue(false)).ConfigureAwait(false);
        }

        protected async Task SelectInputCommandHandler(Command message)
        {
            if (_fullState == null) throw new Exception("Cannot change input source on Denon device becouse device info was not downloaded from device");
            var inputName = (StringValue)message[CommandProperties.InputSource];
            var input = _fullState.TranslateInputName(inputName, _zone.ToString());
            if (input?.Length == 0) throw new Exception($"Input {inputName} was not found on available device input sources");

            await _eventAggregator.QueryWithResultCheckAsync(new DenonControlMessage
            {
                Command = input,
                Api = "formiPhoneAppDirect",
                ReturnNode = "",
                Zone = "",
                Address = _hostName
            }, "").ConfigureAwait(false);

            //TODO Check if this value is ok - confront with pooled state
            _input = await UpdateState(InputSourceState.StateName, _input, inputName).ConfigureAwait(false);
        }

        protected async Task SelectSurroundModeCommandHandler(Command message)
        {
            //Surround support only in main zone
            if (_zone != 1) return;
            var surroundMode = (StringValue)message[CommandProperties.SurroundMode];
            var mode = DenonSurroundModes.MapApiCommand(surroundMode);
            if (mode?.Length == 0) throw new Exception($"Surroundmode {mode} was not found on available surround modes");

            await _eventAggregator.QueryWithResultCheckAsync(new DenonControlMessage
            {
                Command = mode,
                Api = "formiPhoneAppDirect",
                ReturnNode = "",
                Zone = "",
                Address = _hostName
            }, "").ConfigureAwait(false);

            //TODO Check if this value is ok - confront with pooled state
            _surround = await UpdateState(SurroundSoundState.StateName, _surround, surroundMode).ConfigureAwait(false);
        }
    }
}