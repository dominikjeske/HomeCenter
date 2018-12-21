using HomeCenter.Adapters.Sony.Messages;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Queries.Device;
using Proto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.Sony
{
    [ProxyCodeGenerator]
    public abstract class SonyBraviaAdapter : Adapter
    {
        private const int DEFAULT_POOL_INTERVAL = 1000;

        private bool _powerState;
        private double _volume;
        private bool _mute;
        private string _input;
        private string _clientId;
        private string _mac;

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

       

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            _hostname = AsString(MessageProperties.Hostname);
            _authorisationKey = AsString(MessageProperties.AuthKey);
            _clientId = AsString(MessageProperties.ClientID);
            _mac = AsString(MessageProperties.MAC);
            _poolInterval = AsIntTime(MessageProperties.PoolInterval, DEFAULT_POOL_INTERVAL);

           // await ScheduleDeviceRefresh<RefreshStateJob>(_poolInterval).ConfigureAwait(false);
        }

        private SonyControlQuery GetControlCommand(string code)
        {
            return new SonyControlQuery
            {
                Address = _hostname,
                AuthorisationKey = _authorisationKey,
                Code = code
            };
        }

        private SonyJsonQuery GetJsonCommand(string path, string method, object parameters = null)
        {
            return new SonyJsonQuery
            {
                Address = _hostname,
                AuthorisationKey = _authorisationKey,
                Path = path,
                Method = method,
                Params = parameters
            };
        }

         

        protected DiscoveryResponse Discover(DiscoverQuery message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerState(),
                                                               new VolumeState(),
                                                               new MuteState(),
                                                               new InputSourceState()
                                          );
        }

        protected async Task<string> Handle(SonyRegisterQuery sonyRegisterQuery)
        {
            sonyRegisterQuery.Address = _hostname;
            sonyRegisterQuery.ClientID = _clientId;

            var result = await MessageBroker.QueryService<SonyRegisterQuery, string>(sonyRegisterQuery);
            return result;
        }

        protected async Task Handle(RefreshCommand message)
        {
            var cmd = GetJsonCommand("system", "getPowerStatus");
            var power = await MessageBroker.QueryJsonService<SonyJsonQuery, SonyPowerResult>(cmd).ConfigureAwait(false);

            cmd = GetJsonCommand("audio", "getVolumeInformation");
            var audio = await MessageBroker.QueryJsonService<SonyJsonQuery, SonyAudioResult>(cmd).ConfigureAwait(false);

            //TODO save audio and power state
            //_powerState = await UpdateState<BooleanValue>(PowerState.StateName, _powerState, power);
        }

        protected async Task Handle(TurnOnCommand message)
        {
            var command = WakeOnLanCommand.Create(_mac);
            await MessageBroker.SendToService(command).ConfigureAwait(false);
            //var cmd = GetControlCommand("AAAAAQAAAAEAAAAuAw==");

            _powerState = await UpdateState(PowerState.StateName, _powerState, true).ConfigureAwait(false);
        }

        protected async Task Handle(TurnOffCommand message)
        {
            var cmd = GetControlCommand("AAAAAQAAAAEAAAAvAw==");
            await MessageBroker.QueryService<SonyControlQuery, string>(cmd).ConfigureAwait(false);
            _powerState = await UpdateState(PowerState.StateName, _powerState, false).ConfigureAwait(false);
        }

        protected async Task Handle(VolumeUpCommand command)
        {
            var volume = _volume + command.AsDouble(MessageProperties.ChangeFactor);
            var cmd = GetJsonCommand("audio", "setAudioVolume", new SonyAudioVolumeRequest("speaker", ((int)volume).ToString()));
            await MessageBroker.QueryJsonService<SonyJsonQuery, SonyAudioResult>(cmd).ConfigureAwait(false);

            _volume = await UpdateState(VolumeState.StateName, _volume, volume).ConfigureAwait(false);
        }

        protected async Task Handle(VolumeDownCommand command)
        {
            var volume = _volume - command.AsDouble(MessageProperties.ChangeFactor);
            var cmd = GetJsonCommand("audio", "setAudioVolume", new SonyAudioVolumeRequest("speaker", ((int)volume).ToString()));
            await MessageBroker.QueryJsonService<SonyJsonQuery, SonyAudioResult>(cmd).ConfigureAwait(false);

            _volume = await UpdateState(VolumeState.StateName, _volume, volume).ConfigureAwait(false);
        }

        protected async Task Handle(VolumeSetCommand command)
        {
            var volume = command.AsDouble(MessageProperties.Value);
            var cmd = GetJsonCommand("audio", "setAudioVolume", new SonyAudioVolumeRequest("speaker", ((int)volume).ToString()));
            await MessageBroker.QueryJsonService<SonyJsonQuery, SonyAudioResult>(cmd).ConfigureAwait(false);

            _volume = await UpdateState(VolumeState.StateName, _volume, volume).ConfigureAwait(false);
        }

        protected async Task Handle(MuteCommand message)
        {
            var cmd = GetJsonCommand("audio", "setAudioMute", new SonyAudioMuteRequest(true));
            await MessageBroker.QueryJsonService<SonyJsonQuery, SonyAudioResult>(cmd).ConfigureAwait(false);

            _mute = await UpdateState(MuteState.StateName, _mute, true).ConfigureAwait(false);
        }

        protected async Task Handle(UnmuteCommand message)
        {
            var cmd = GetJsonCommand("audio", "setAudioMute", new SonyAudioMuteRequest(false));
            await MessageBroker.QueryJsonService<SonyJsonQuery, SonyAudioResult>(cmd).ConfigureAwait(false);

            _mute = await UpdateState(MuteState.StateName, _mute, false).ConfigureAwait(false);
        }

        protected async Task Handle(InputSetCommand message)
        {
            var inputName = message.AsString(MessageProperties.InputSource);
            if (!_inputSourceMap.ContainsKey(inputName)) throw new UnsupportedPropertyStateException($"Input {inputName} was not found on available device input sources");

            var code = _inputSourceMap[inputName];

            var cmd = GetControlCommand(code);
            await MessageBroker.QueryService<SonyControlQuery, string>(cmd).ConfigureAwait(false);

            _input = await UpdateState(InputSourceState.StateName, _input, inputName).ConfigureAwait(false);
        }

       
    }
}