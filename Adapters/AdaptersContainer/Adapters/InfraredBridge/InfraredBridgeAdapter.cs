using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Capabilities;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Commands.Responses;
using Wirehome.ComponentModel.Events;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core.Interface.Native;
using Wirehome.Core.Services;
using Wirehome.Core.Services.I2C;
using Wirehome.Model.Events;
using Wirehome.Model.Extensions;

namespace Wirehome.ComponentModel.Adapters.Denon
{
    public class InfraredBridgeAdapter : Adapter
    {
        private const int DEAFULT_REPEAT = 3;
        private IntValue _pinNumber;
        private IntValue _I2cAddress;

        private readonly ISerialMessagingService _serialMessagingService;
        private readonly II2CBusService _i2cServiceBus;
        private readonly Dictionary<IntValue, BooleanValue> _state = new Dictionary<IntValue, BooleanValue>();

        public InfraredBridgeAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
            _serialMessagingService = adapterServiceFactory.GetUartService();
            _i2cServiceBus = adapterServiceFactory.GetI2CService();
        }

        public override async Task Initialize()
        {
            if (!IsEnabled) return;

            await base.Initialize().ConfigureAwait(false);

            _I2cAddress = this[AdapterProperties.I2cAddress].AsInt();
            _pinNumber = this[AdapterProperties.PinNumber].AsInt();

            _serialMessagingService.RegisterMessageHandler(SerialHandler);
        }

        public async Task<bool> SerialHandler(byte messageType, byte messageSize, IBinaryReader reader)
        {
            if (messageType == 3 && messageSize == 6)
            {
                var system = reader.ReadByte();
                var code = reader.ReadUInt32();
                var bits = reader.ReadByte();

                await _eventAggregator.PublishDeviceEvent(new InfraredEvent(Uid, system, (int)code)).ConfigureAwait(false);

                return true;
            }
            return false;
        }

        protected Task SendCodeCommandHandler(Command message)
        {
            //TODO uint?
            var commandCode = message[CommandProperties.Code].AsInt();
            var system = message[CommandProperties.System].AsInt();
            var bits = message[CommandProperties.Bits].AsInt();
            var repeat = base.GetPropertyValue(CommandProperties.Repeat, new IntValue(DEAFULT_REPEAT)).AsInt();

            var package = new List<byte>
            {
                3,
                (byte)repeat,
                (byte)system,
                (byte)bits
            };
            package.AddRange(BitConverter.GetBytes(commandCode));
            var code = package.ToArray();

            _i2cServiceBus.Write(I2CSlaveAddress.FromValue(_I2cAddress), package.ToArray());

            return Task.CompletedTask;
        }

        protected DiscoveryResponse DiscoverCapabilitiesHandler(Command message)
        {
            return new DiscoveryResponse(new List<EventSource> { new EventSource(EventType.InfraredCode, EventDirections.Recieving),
                                                                 new EventSource(EventType.InfraredCode, EventDirections.Sending)}, new PowerState());
        }
    }
}