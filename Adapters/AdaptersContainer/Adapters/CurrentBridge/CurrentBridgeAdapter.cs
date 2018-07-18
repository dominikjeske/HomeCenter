﻿using System.Collections.Generic;
using System.Threading.Tasks;
using HomeCenter.ComponentModel.Capabilities;
using HomeCenter.ComponentModel.Commands;
using HomeCenter.ComponentModel.Commands.Responses;
using HomeCenter.ComponentModel.ValueTypes;
using HomeCenter.Core.Interface.Native;
using HomeCenter.Core.Services;
using HomeCenter.Model.ComponentModel.Capabilities.Constants;
using HomeCenter.Model.Extensions;

namespace HomeCenter.ComponentModel.Adapters.Denon
{
    public class CurrentBridgeAdapter : Adapter
    {
        private readonly ISerialMessagingService _serialMessagingService;
        private readonly Dictionary<IntValue, IntValue> _state = new Dictionary<IntValue, IntValue>();

        public CurrentBridgeAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
            _serialMessagingService = adapterServiceFactory.GetUartService();
            _requierdProperties.Add(AdapterProperties.PinNumber);
        }

        public override async Task Initialize()
        {
            await base.Initialize().ConfigureAwait(false);

            var _i2cAddress = this[AdapterProperties.I2cAddress].AsInt();

            foreach (var val in this[AdapterProperties.UsedPins].AsStringList())
            {
                _state.Add(IntValue.FromString(val), 0);
            }

            if (!IsEnabled) return;

            _serialMessagingService.RegisterMessageHandler(MessageHandler);
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

        protected DiscoveryResponse DiscoverCapabilitiesHandler(Command message)
        {
            return new DiscoveryResponse(RequierdProperties(), new CurrentState(ReadWriteModeValues.Read));
        }
    }
}