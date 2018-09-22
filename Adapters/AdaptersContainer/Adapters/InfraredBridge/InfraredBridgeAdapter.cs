using HomeCenter.CodeGeneration;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Commands;
using HomeCenter.Model.Commands.Responses;
using HomeCenter.Model.Events;
using HomeCenter.Model.ValueTypes;
using HomeCenter.Core.Interface.Native;
using HomeCenter.Core.Services;
using HomeCenter.Core.Services.I2C;
using HomeCenter.Model.Commands.Device;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Queries.Device;
using Proto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Model.Adapters.Denon
{
    [ProxyCodeGenerator]
    public abstract class InfraredBridgeAdapter : Adapter
    {
        private const int DEAFULT_REPEAT = 3;
        private IntValue _pinNumber;
        private IntValue _I2cAddress;


        private readonly II2CBusService _i2cServiceBus;
        private readonly Dictionary<IntValue, BooleanValue> _state = new Dictionary<IntValue, BooleanValue>();

        protected InfraredBridgeAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
            //TODO register handler
            _i2cServiceBus = adapterServiceFactory.GetI2CService();
        }

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            _I2cAddress = this[AdapterProperties.I2cAddress].AsInt();
            _pinNumber = this[AdapterProperties.PinNumber].AsInt();

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

        protected Task SendCode(SendCodeCommand message)
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

        protected DiscoveryResponse Discover(DiscoverQuery message)
        {
            return new DiscoveryResponse(new List<EventSource> { new EventSource(EventType.InfraredCode, EventDirections.Recieving),
                                                                 new EventSource(EventType.InfraredCode, EventDirections.Sending)}, new PowerState());
        }
    }
}