using System;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Capabilities;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Commands.Responses;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Model.Extensions;

namespace Wirehome.ComponentModel.Adapters.Kodi
{
    public class KodiAdapter : Adapter
    {
        public const int DEFAULT_POOL_INTERVAL = 1000;

        private string _hostname;
        private TimeSpan _poolInterval;
        private int _port;
        private string _userName;
        private string _Password;

        private BooleanValue _powerState;
        private DoubleValue _volume;
        private BooleanValue _mute;
        private DoubleValue _speed;

        //TODO read this value in refresh?
        private int? PlayerId { get; }

        public KodiAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
        }

        public override async Task Initialize()
        {
            if (!IsEnabled) return;
            await base.Initialize().ConfigureAwait(false);

            _hostname = this[AdapterProperties.Hostname].AsString();
            _port = this[AdapterProperties.Port].AsInt();
            _userName = this[AdapterProperties.UserName].AsString();
            _Password = this[AdapterProperties.Password].AsString();
            _poolInterval = GetPropertyValue(AdapterProperties.PoolInterval, new IntValue(DEFAULT_POOL_INTERVAL)).AsIntTimeSpan();

            await ScheduleDeviceRefresh<RefreshStateJob>(_poolInterval).ConfigureAwait(false);
        }

        protected Task RefreshCommandHandler(Command message)
        {
            //TODO pool state
            return Task.CompletedTask;
        }

        protected DiscoveryResponse DiscoverCapabilitiesHandler(Command message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerState(),
                                                               new VolumeState(),
                                                               new MuteState(),
                                                               new PlaybackState()
                                          );
        }

        protected async Task TurnOnCommandHandler(Command message)
        {
            //await _eventAggregator.QueryAsync<ComputerControlMessage, string>(new ComputerControlMessage
            //{
            //    Address = _hostname,
            //    Service = "Process",
            //    Message = new ProcessPost { ProcessName = "kodi", Start = true },
            //    Port = 5000
            //});
            _powerState = await UpdateState(PowerState.StateName, _powerState, new BooleanValue(true)).ConfigureAwait(false);
        }

        protected async Task TurnOffCommandHandler(Command message)
        {
            var result = await _eventAggregator.QueryAsync<KodiMessage, string>(new KodiMessage
            {
                Address = _hostname,
                UserName = _userName,
                Password = _Password,
                Port = _port,
                Method = "Application.Quit"
            }).ConfigureAwait(false);
            _powerState = await UpdateState(PowerState.StateName, _powerState, new BooleanValue(false)).ConfigureAwait(false);
        }

        protected async Task VolumeUpCommandHandler(Command command)
        {
            var volume = _volume + command[CommandProperties.ChangeFactor].AsDouble();

            var result = await _eventAggregator.QueryAsync<KodiMessage, string>(new KodiMessage
            {
                Address = _hostname,
                UserName = _userName,
                Password = _Password,
                Port = _port,
                Method = "Application.SetVolume",
                Parameters = new { volume = (int)volume }
            }).ConfigureAwait(false);

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume)).ConfigureAwait(false);
        }

        protected async Task VolumeDownCommandHandler(Command command)
        {
            var volume = _volume + command[CommandProperties.ChangeFactor].AsDouble();

            var result = await _eventAggregator.QueryAsync<KodiMessage, string>(new KodiMessage
            {
                Address = _hostname,
                UserName = _userName,
                Password = _Password,
                Port = _port,
                Method = "Application.SetVolume",
                Parameters = new { volume = (int)volume }
            }).ConfigureAwait(false);

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume)).ConfigureAwait(false);
        }

        protected async Task VolumeSetCommandHandler(Command command)
        {
            var volume = command[CommandProperties.Value].AsDouble();
            var result = await _eventAggregator.QueryAsync<KodiMessage, string>(new KodiMessage
            {
                Address = _hostname,
                UserName = _userName,
                Password = _Password,
                Port = _port,
                Method = "Application.SetVolume",
                Parameters = new { volume = volume }
            }).ConfigureAwait(false);

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume)).ConfigureAwait(false);
        }

        protected async Task MuteCommandHandler(Command message)
        {
            var result = await _eventAggregator.QueryAsync<KodiMessage, string>(new KodiMessage
            {
                Address = _hostname,
                UserName = _userName,
                Password = _Password,
                Port = _port,
                Method = "Application.SetMute",
                Parameters = new { mute = true }
            }).ConfigureAwait(false);

            _mute = await UpdateState(MuteState.StateName, _mute, new BooleanValue(true)).ConfigureAwait(false);
        }

        protected async Task UnmuteCommandHandler(Command message)
        {
            var result = await _eventAggregator.QueryAsync<KodiMessage, string>(new KodiMessage
            {
                Address = _hostname,
                UserName = _userName,
                Password = _Password,
                Port = _port,
                Method = "Application.SetMute",
                Parameters = new { mute = false }
            }).ConfigureAwait(false);

            _mute = await UpdateState(MuteState.StateName, _mute, new BooleanValue(false)).ConfigureAwait(false);
        }

        protected async Task PlayCommandHandler(Command message)
        {
            if (_speed != 0) return;

            //{"jsonrpc": "2.0", "method": "Player.PlayPause", "params": { "playerid": 1 }, "id": 1}
            var result = await _eventAggregator.QueryAsync<KodiMessage, string>(new KodiMessage
            {
                Address = _hostname,
                UserName = _userName,
                Password = _Password,
                Port = _port,
                Method = "Player.PlayPause",
                Parameters = new { playerid = PlayerId.GetValueOrDefault() }
            }).ConfigureAwait(false);

            _speed = await UpdateState(PlaybackState.StateName, _speed, new DoubleValue(1.0)).ConfigureAwait(false);
        }

        protected async Task StopCommandHandler(Command message)
        {
            if (_speed != 0) return;

            //{ "jsonrpc": "2.0", "method": "Player.Stop", "id": "libMovies", "params": { "playerid": 1 } }
            var result = await _eventAggregator.QueryAsync<KodiMessage, string>(new KodiMessage
            {
                Address = _hostname,
                UserName = _userName,
                Password = _Password,
                Port = _port,
                Method = "Player.Stop",
                Parameters = new { playerid = PlayerId.GetValueOrDefault() }
            }).ConfigureAwait(false);

            _speed = await UpdateState(PlaybackState.StateName, _speed, new DoubleValue(1.0)).ConfigureAwait(false);
        }
    }
}