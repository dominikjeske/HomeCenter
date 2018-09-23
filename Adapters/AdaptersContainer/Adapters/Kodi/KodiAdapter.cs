using HomeCenter.CodeGeneration;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Commands.Responses;
using HomeCenter.Model.ValueTypes;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages.Queries.Device;
using Proto;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Model.Adapters.Kodi
{
    [ProxyCodeGenerator]
    public abstract class KodiAdapter : Adapter
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

        protected KodiAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
        }

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            _hostname = this[AdapterProperties.Hostname].AsString();
            _port = this[AdapterProperties.Port].AsInt();
            _userName = this[AdapterProperties.UserName].AsString();
            _Password = this[AdapterProperties.Password].AsString();
            _poolInterval = GetPropertyValue(AdapterProperties.PoolInterval, new IntValue(DEFAULT_POOL_INTERVAL)).AsIntTimeSpan();

            await ScheduleDeviceRefresh<RefreshStateJob>(_poolInterval).ConfigureAwait(false);
        }

        protected Task Refresh(RefreshCommand message)
        {
            //TODO pool state
            return Task.CompletedTask;
        }

        protected DiscoveryResponse Discover(DiscoverQuery message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerState(),
                                                               new VolumeState(),
                                                               new MuteState(),
                                                               new PlaybackState()
                                          );
        }

        protected async Task TurnOn(TurnOnCommand message)
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

        protected async Task TurnOff(TurnOffCommand message)
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

        protected async Task VolumeUp(VolumeUpCommand command)
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

        protected async Task VolumeDown(VolumeDownCommand command)
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

        protected async Task VolumeSet(VolumeSetCommand command)
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

        protected async Task Mute(MuteCommand message)
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

        protected async Task Unmute(UnmuteCommand message)
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

        protected async Task Play(PlayCommand message)
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

        protected async Task Stop(StopCommand message)
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