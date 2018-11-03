using HomeCenter.Adapters.Kodi.Messages;
using HomeCenter.Adapters.Kodi.Messages.JsonModels;
using HomeCenter.Adapters.PC.Messages;
using HomeCenter.Adapters.PC.Model;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Model.ValueTypes;
using Proto;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.Kodi
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

        private KodiCommand GetKodiCommand(string method, object parameters = null)
        {
            return new KodiCommand
            {
                Address = _hostname,
                UserName = _userName,
                Password = _Password,
                Port = _port,
                Method = method,
                Parameters = parameters
            };
        }

        protected async Task Handle(TurnOnCommand message)
        {
            var cmd = new ComputerCommand
            {
                Address = _hostname,
                Service = "Process",
                Message = new ProcessPost { ProcessName = "kodi", Start = true },
                Port = 5000
            };
            await MessageBroker.SendToService(cmd).ConfigureAwait(false);
            _powerState = await UpdateState(PowerState.StateName, _powerState, new BooleanValue(true)).ConfigureAwait(false);
        }

        protected async Task Handle(TurnOffCommand message)
        {
            var cmd = GetKodiCommand("Application.Quit");
            var result = await MessageBroker.QueryService<KodiCommand, JsonRpcResponse>(cmd).ConfigureAwait(false);
            _powerState = await UpdateState(PowerState.StateName, _powerState, new BooleanValue(false)).ConfigureAwait(false);
        }

        protected async Task Handle(VolumeUpCommand command)
        {
            var volume = _volume + command[CommandProperties.ChangeFactor].AsDouble();
            var cmd = GetKodiCommand("Application.SetVolume", new { volume = (int)volume });
            var result = await MessageBroker.QueryService<KodiCommand, JsonRpcResponse>(cmd).ConfigureAwait(false);
            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume)).ConfigureAwait(false);
        }

        protected async Task Handle(VolumeDownCommand command)
        {
            var volume = _volume - command[CommandProperties.ChangeFactor].AsDouble();
            var cmd = GetKodiCommand("Application.SetVolume", new { volume = (int)volume });
            var result = await MessageBroker.QueryService<KodiCommand, JsonRpcResponse>(cmd).ConfigureAwait(false);

            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume)).ConfigureAwait(false);
        }

        protected async Task Handle(VolumeSetCommand command)
        {
            var volume = command[CommandProperties.Value].AsDouble();
            var cmd = GetKodiCommand("Application.SetVolume", new { volume });
            var result = await MessageBroker.QueryService<KodiCommand, JsonRpcResponse>(cmd).ConfigureAwait(false);
            _volume = await UpdateState(VolumeState.StateName, _volume, new DoubleValue(volume)).ConfigureAwait(false);
        }

        protected async Task Handle(MuteCommand message)
        {
            var cmd = GetKodiCommand("Application.SetMute", new { mute = true });
            var result = await MessageBroker.QueryService<KodiCommand, JsonRpcResponse>(cmd).ConfigureAwait(false);
            _mute = await UpdateState(MuteState.StateName, _mute, new BooleanValue(true)).ConfigureAwait(false);
        }

        protected async Task Handle(UnmuteCommand message)
        {
            var cmd = GetKodiCommand("Application.SetMute", new { mute = false });
            var result = await MessageBroker.QueryService<KodiCommand, JsonRpcResponse>(cmd).ConfigureAwait(false);
            _mute = await UpdateState(MuteState.StateName, _mute, new BooleanValue(false)).ConfigureAwait(false);
        }

        protected async Task Handle(PlayCommand message)
        {
            //{"jsonrpc": "2.0", "method": "Player.PlayPause", "params": { "playerid": 1 }, "id": 1}
            if (_speed != 0) return;

            var cmd = GetKodiCommand("Player.PlayPause", new { playerid = PlayerId.GetValueOrDefault() });
            var result = await MessageBroker.QueryService<KodiCommand, JsonRpcResponse>(cmd).ConfigureAwait(false);
            _speed = await UpdateState(PlaybackState.StateName, _speed, new DoubleValue(1.0)).ConfigureAwait(false);
        }

        protected async Task Handle(StopCommand message)
        {
            //{ "jsonrpc": "2.0", "method": "Player.Stop", "id": "libMovies", "params": { "playerid": 1 } }
            if (_speed != 0) return;

            var cmd = GetKodiCommand("Player.Stop", new { playerid = PlayerId.GetValueOrDefault() });
            var result = await MessageBroker.QueryService<KodiCommand, JsonRpcResponse>(cmd).ConfigureAwait(false);
            _speed = await UpdateState(PlaybackState.StateName, _speed, new DoubleValue(1.0)).ConfigureAwait(false);
        }
    }
}