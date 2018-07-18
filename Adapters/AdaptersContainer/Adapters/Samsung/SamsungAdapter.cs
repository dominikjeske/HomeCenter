using System;
using System.Threading.Tasks;
using HomeCenter.ComponentModel.Capabilities;
using HomeCenter.ComponentModel.Commands;
using HomeCenter.ComponentModel.Commands.Responses;
using HomeCenter.ComponentModel.ValueTypes;
using HomeCenter.Core.Extensions;
using HomeCenter.Model.Extensions;

namespace HomeCenter.ComponentModel.Adapters.Samsung
{
    public class SamsungAdapter : Adapter
    {
        private string _hostname;

        private BooleanValue _powerState;
        //private DoubleValue _volume;
        private BooleanValue _mute;
        private StringValue _input;

        public SamsungAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
        }

        public override async Task Initialize()
        {
            if (!IsEnabled) return;
            await base.Initialize().ConfigureAwait(false);

            _hostname = this[AdapterProperties.Hostname].AsString();
        }

        protected DiscoveryResponse DiscoverCapabilitiesHandler(Command message)
        {
            //TODO Add read only state
            return new DiscoveryResponse(RequierdProperties(), new PowerState(),
                                                               new VolumeState(),
                                                               new MuteState(),
                                                               new InputSourceState()
                                          );
        }

        protected Task TurnOnCommandHandler(Command message)
        {
            //TODO ADD infrared message
            return Task.CompletedTask;
        }

        protected async Task TurnOffCommandHandler(Command message)
        {
            await _eventAggregator.QueryAsync<SamsungControlMessage, string>(new SamsungControlMessage
            {
                Address = _hostname,
                Code = "KEY_POWEROFF"
            }).ConfigureAwait(false);
            _powerState = await UpdateState(PowerState.StateName, _powerState, new BooleanValue(false)).ConfigureAwait(false);
        }

        protected Task VolumeUpCommandHandler(Command command)
        {
            return _eventAggregator.QueryAsync<SamsungControlMessage, string>(new SamsungControlMessage
            {
                Address = _hostname,
                Code = "KEY_VOLUP"
            });
        }

        protected Task VolumeDownCommandHandler(Command command)
        {
            return _eventAggregator.QueryAsync<SamsungControlMessage, string>(new SamsungControlMessage
            {
                Address = _hostname,
                Code = "KEY_VOLDOWN"
            });
        }

        protected async Task MuteCommandHandler(Command message)
        {
            await _eventAggregator.QueryAsync<SamsungControlMessage, string>(new SamsungControlMessage
            {
                Address = _hostname,
                Code = "KEY_MUTE"
            }).ConfigureAwait(false);

            _mute = await UpdateState(MuteState.StateName, _mute, new BooleanValue(!_mute)).ConfigureAwait(false);
        }

        protected async Task SelectInputCommandHandler(Command message)
        {
            var inputName = (StringValue)message[CommandProperties.InputSource];

            var source = "";
            if (inputName == "HDMI")
            {
                source = "KEY_HDMI";
            }
            else if (inputName == "AV")
            {
                source = "KEY_AV1";
            }
            else if (inputName == "COMPONENT")
            {
                source = "KEY_COMPONENT1";
            }
            else if (inputName == "TV")
            {
                source = "KEY_TV";
            }

            if (source?.Length == 0) throw new Exception($"Input {inputName} was not found on Samsung available device input sources");

            await _eventAggregator.QueryAsync<SamsungControlMessage, string>(new SamsungControlMessage
            {
                Address = _hostname,
                Code = source
            }).ConfigureAwait(false);

            _input = await UpdateState(InputSourceState.StateName, _input, inputName).ConfigureAwait(false);
        }
    }
}