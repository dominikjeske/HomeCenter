using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters.Kodi;
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
    public abstract class CurrentBridgeAdapter : Adapter
    {
        //TODO register handler
        private readonly Dictionary<IntValue, IntValue> _state = new Dictionary<IntValue, IntValue>();

        protected CurrentBridgeAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
            _requierdProperties.Add(AdapterProperties.PinNumber);

            //KodiAdapterProxy
        }


        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            var _i2cAddress = this[AdapterProperties.I2cAddress].AsInt();

            foreach (var val in this[AdapterProperties.UsedPins].AsStringList())
            {
                _state.Add(IntValue.FromString(val), 0);
            }

            if (!IsEnabled) return;

        }


        public async Task<bool> MessageHandler(byte messageType, byte messageSize, IBinaryReader reader)
        {
            if (messageType == 5 && messageSize == 2)
            {
                var pin = reader.ReadByte();
                var currentExists = reader.ReadByte();

                _state[pin] = await UpdateState(CurrentState.StateName, pin, (IntValue)currentExists).ConfigureAwait(false);

                return true;
            }
            return false;
        }

        protected DiscoveryResponse DeviceDiscoveryQuery(DiscoverQuery message)
        {
            return new DiscoveryResponse(RequierdProperties(), new CurrentState(ReadWriteModeValues.Read));
        }
    }
}