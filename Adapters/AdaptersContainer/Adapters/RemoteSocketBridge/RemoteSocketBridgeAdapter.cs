using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HomeCenter.ComponentModel.Capabilities;
using HomeCenter.ComponentModel.Capabilities.Constants;
using HomeCenter.ComponentModel.Commands;
using HomeCenter.ComponentModel.Commands.Responses;
using HomeCenter.ComponentModel.Events;
using HomeCenter.ComponentModel.ValueTypes;
using HomeCenter.Messaging;
using HomeCenter.Core.Extensions;
using HomeCenter.Core.Hardware.RemoteSockets;
using HomeCenter.Core.Interface.Native;
using HomeCenter.Core.Services;
using HomeCenter.Core.Services.I2C;
using HomeCenter.Model.Events;
using HomeCenter.Model.Extensions;

namespace HomeCenter.ComponentModel.Adapters.Denon
{
    public class RemoteSocketBridgeAdapter : Adapter
    {
        private const int DEFAULT_REPEAT = 3;
        private IntValue _pinNumber;
        private IntValue _I2cAddress;

        private readonly ISerialMessagingService _serialMessagingService;
        private readonly II2CBusService _i2cServiceBus;

        private readonly Dictionary<StringValue, StringValue> _state = new Dictionary<StringValue, StringValue>();

        public RemoteSocketBridgeAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
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

            _disposables.Add(_eventAggregator.SubscribeForDeviceQuery<DeviceCommand>(DeviceCommandHandler, Uid));
        }

        public async Task<bool> SerialHandler(byte messageType, byte messageSize, IBinaryReader reader)
        {
            if (messageType == 2 && messageSize == 6)
            {
                var code = reader.ReadUInt32();
                var bits = reader.ReadByte();
                var protocol = reader.ReadByte();

                var dipswitchCode = DipswitchCode.ParseCode(code);

                if (dipswitchCode != null)
                {
                    await _eventAggregator.PublishDeviceEvent(new DipswitchEvent(Uid, dipswitchCode)).ConfigureAwait(false);
                }

                return true;
            }
            return false;
        }

        private Task<object> DeviceCommandHandler(IMessageEnvelope<DeviceCommand> messageEnvelope)
        {
            return ExecuteCommand(messageEnvelope.Message);
        }

        protected async Task TurnOnCommandHandler(Command message)
        {
            byte[] package = PreparePackage(message, nameof(RemoteSocketCommand.TurnOn), out var dipswitchCode);

            if (_i2cServiceBus.Write(I2CSlaveAddress.FromValue((byte)_I2cAddress.Value), package).Status == I2cTransferStatus.FullTransfer)
            {
                await UpdateState(dipswitchCode).ConfigureAwait(false);
            }
        }

        protected async Task TurnOffCommandHandler(Command message)
        {
            byte[] package = PreparePackage(message, nameof(RemoteSocketCommand.TurnOff), out var dipswitchCode);

            if (_i2cServiceBus.Write(I2CSlaveAddress.FromValue(_I2cAddress), package).Status == I2cTransferStatus.FullTransfer)
            {
                await UpdateState(dipswitchCode).ConfigureAwait(false);
            }
        }

        private byte[] PreparePackage(Command message, string commandName, out DipswitchCode dipswitchCode)
        {
            var system = message[CommandProperties.System].AsString();
            var unit = message[CommandProperties.Unit].AsString();
            var repeat = GetPropertyValue(CommandProperties.Repeat, new IntValue(DEFAULT_REPEAT)).AsInt();
            dipswitchCode = DipswitchCode.ParseCode(system, unit, commandName);
            var package = new byte[8];
            package[0] = 2;

            var code = BitConverter.GetBytes(dipswitchCode.Code);
            Array.Copy(code, 0, package, 1, 4);

            package[5] = 24;
            package[6] = (byte)repeat;
            package[7] = (byte)_pinNumber;

            return package;
        }

        private async Task UpdateState(DipswitchCode code)
        {
            _state[code.ToShortCode()] = await UpdateState(PowerState.StateName, _state.ElementAtOrNull(code.ToShortCode()), new StringValue(PowerStateValue.ON)).ConfigureAwait(false);
        }

        protected DiscoveryResponse DiscoverCapabilitiesHandler(Command message)
        {
            return new DiscoveryResponse(new List<EventSource> { new EventSource(EventType.DipswitchCode, EventDirections.Recieving),
                                                                 new EventSource(EventType.DipswitchCode, EventDirections.Sending)}, new PowerState());
        }
    }
}