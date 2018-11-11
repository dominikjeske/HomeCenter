using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Model.Messages.Queries.Service;
using Proto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.CurrentBridge
{
    [ProxyCodeGenerator]
    public abstract class CurrentBridgeAdapter : Adapter
    {
        private readonly Dictionary<byte, byte> _state = new Dictionary<byte, byte>();

        protected CurrentBridgeAdapter()
        {
            _requierdProperties.Add(AdapterProperties.PinNumber);
        }

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            var _i2cAddress = AsInt(AdapterProperties.I2cAddress);

            foreach (var val in AsList(AdapterProperties.UsedPins))
            {
                _state.Add(byte.Parse(val), 0);
            }

            var registration = new SerialRegistrationCommand(Self, 5, new Format[]
            {
                new Format(1, typeof(byte), "Pin"),
                new Format(2, typeof(byte), "Current")
            });

            await MessageBroker.SendToService(registration).ConfigureAwait(false);
        }

        protected async Task Handle(SerialResultEvent serialResult)
        {
            var pin = serialResult.AsByte("Pin");
            var currentExists = serialResult.AsByte("Current");
            var previousValue = _state[pin];

            _state[pin] = await UpdateState(HumidityState.StateName, previousValue, currentExists).ConfigureAwait(false);
        }

        protected DiscoveryResponse DeviceDiscoveryQuery(DiscoverQuery message)
        {
            return new DiscoveryResponse(RequierdProperties(), new CurrentState(ReadWriteMode.Read));
        }
    }
}