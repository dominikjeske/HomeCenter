using HomeCenter.CodeGeneration;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Commands.Responses;
using HomeCenter.Model.ValueTypes;
using HomeCenter.Core.Interface.Native;
using HomeCenter.Core.Services;
using HomeCenter.Model.ComponentModel.Capabilities.Constants;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Queries.Device;
using Proto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Model.Adapters.Denon
{
    [ProxyCodeGenerator]
    public abstract class HumidityBridgeAdapter : Adapter
    {
        //TODO register handler
        private readonly Dictionary<IntValue, DoubleValue> _state = new Dictionary<IntValue, DoubleValue>();

        protected HumidityBridgeAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
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

        }

        public async Task<bool> MessageHandler(byte messageType, byte messageSize, IBinaryReader reader)
        {
            if (messageType == 6 && messageSize == 5)
            {
                var pin = reader.ReadByte();
                var humidity = reader.ReadSingle();

                _state[pin] = await UpdateState(HumidityState.StateName, pin, (DoubleValue)humidity).ConfigureAwait(false);

                return true;
            }
            return false;
        }

        protected DiscoveryResponse Discover(DiscoverQuery message)
        {
            return new DiscoveryResponse(RequierdProperties(), new HumidityState(ReadWriteModeValues.Read));
        }
    }
}