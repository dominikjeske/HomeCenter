using HomeCenter.CodeGeneration;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Commands.Responses;
using HomeCenter.Model.Commands.Serial;
using HomeCenter.Model.ComponentModel.Capabilities.Constants;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Queries.Device;
using HomeCenter.Model.ValueTypes;
using Proto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Model.Adapters.Denon
{
    [ProxyCodeGenerator]
    public abstract class CurrentBridgeAdapter : Adapter
    {
        private readonly Dictionary<IntValue, IntValue> _state = new Dictionary<IntValue, IntValue>();

        protected CurrentBridgeAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
            _requierdProperties.Add(AdapterProperties.PinNumber);
        }

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            var _i2cAddress = this[AdapterProperties.I2cAddress].AsInt();

            foreach (var val in this[AdapterProperties.UsedPins].AsStringList())
            {
                _state.Add(IntValue.FromString(val), 0);
            }

            var registration = new SerialRegistrationCommand(Self, 5, new Format[]
            {
                new Format(1, typeof(byte), "Pin"),
                new Format(2, typeof(byte), "Current")
            });
            //TODO Send
        }

        protected void Handle(SerialResultCommand serialResultCommand)
        {
            //_state[pin] = await UpdateState(CurrentState.StateName, pin, (IntValue)currentExists).ConfigureAwait(false);
        }

        protected DiscoveryResponse DeviceDiscoveryQuery(DiscoverQuery message)
        {
            return new DiscoveryResponse(RequierdProperties(), new CurrentState(ReadWriteModeValues.Read));
        }
    }
}