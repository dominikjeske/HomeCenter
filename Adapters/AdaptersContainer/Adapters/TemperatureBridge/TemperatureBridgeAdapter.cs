using System.Collections.Generic;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Capabilities;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Commands.Responses;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core.Interface.Native;
using Wirehome.Core.Services;
using Wirehome.Model.ComponentModel.Capabilities.Constants;
using Wirehome.Model.Extensions;

namespace Wirehome.ComponentModel.Adapters.Denon
{
    public class TemperatureBridgeAdapter : Adapter
    {
        private readonly ISerialMessagingService _serialMessagingService;
        private Dictionary<IntValue, DoubleValue> _state = new Dictionary<IntValue, DoubleValue>();

        public TemperatureBridgeAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
            _serialMessagingService = adapterServiceFactory.GetUartService();
            _requierdProperties.Add(AdapterProperties.PinNumber);
        }

        public override async Task Initialize()
        {
            if (!IsEnabled) return;
            await base.Initialize().ConfigureAwait(false);

            var _i2cAddress = this[AdapterProperties.I2cAddress].AsInt();

            foreach(var val in this[AdapterProperties.UsedPins].AsStringList())
            {
                _state.Add(IntValue.FromString(val), 0);
            }
            _serialMessagingService.RegisterMessageHandler(MessageHandler);
        }

        public async Task<bool> MessageHandler(byte messageType, byte messageSize, IBinaryReader reader)
        {
            if (messageType == 1 && messageSize == 5)
            {
                var pin = reader.ReadByte();
                var temperature = reader.ReadSingle();

                _state[pin] = await UpdateState(TemperatureState.StateName, pin, (DoubleValue)temperature).ConfigureAwait(false);

                return true;
            }
            return false;
        }

        protected DiscoveryResponse DiscoverCapabilitiesHandler(Command message)
        {
            return new DiscoveryResponse(RequierdProperties(), new TemperatureState(ReadWriteModeValues.Read));
        }
    }
}