using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Codes;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Events;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Model.Messages.Queries.Service;
using HomeCenter.Model.ValueTypes;
using HomeCenter.Utils.Extensions;
using Proto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.RemoteSocketBridge
{
    [ProxyCodeGenerator]
    public abstract class RemoteSocketBridgeAdapter : Adapter
    {
        private const int DEFAULT_REPEAT = 3;
        private IntValue _pinNumber;
        private IntValue _I2cAddress;

        private readonly Dictionary<StringValue, BooleanValue> _state = new Dictionary<StringValue, BooleanValue>();

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            _I2cAddress = this[AdapterProperties.I2cAddress].AsInt();
            _pinNumber = this[AdapterProperties.PinNumber].AsInt();

            var registration = new SerialRegistrationQuery(Self, 2, new Format[]
             {
                    new Format(1, typeof(uint), "Code"),
                    new Format(2, typeof(byte), "Bits"),
                    new Format(3, typeof(byte), "Protocol")
             });
            //TODO Send
            //TODO count size??
        }

        protected void Handle(SerialResultCommand serialResultCommand)
        {
            //var dipswitchCode = DipswitchCode.ParseCode(code);

            //if (dipswitchCode != null)
            //{
            //    await _eventAggregator.PublishDeviceEvent(new DipswitchEvent(Uid, dipswitchCode)).ConfigureAwait(false);
            //}
        }

        protected async Task TurnOn(TurnOnCommand message)
        {
            byte[] package = PreparePackage(message, nameof(RemoteSocketCommand.TurnOn), out var dipswitchCode);

            //if (_i2cServiceBus.Write(I2CSlaveAddress.FromValue((byte)_I2cAddress.Value), package).Status == I2cTransferStatus.FullTransfer)
            //{
            //    await UpdateState(dipswitchCode).ConfigureAwait(false);
            //}
        }

        protected async Task TurnOff(TurnOffCommand message)
        {
            byte[] package = PreparePackage(message, nameof(RemoteSocketCommand.TurnOff), out var dipswitchCode);

            //if (_i2cServiceBus.Write(I2CSlaveAddress.FromValue(_I2cAddress), package).Status == I2cTransferStatus.FullTransfer)
            //{
            //    await UpdateState(dipswitchCode).ConfigureAwait(false);
            //}
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
            _state[code.ToShortCode()] = await UpdateState(PowerState.StateName, _state.ElementAtOrNull(code.ToShortCode()), new BooleanValue(true)).ConfigureAwait(false);
        }

        protected DiscoveryResponse Discover(DiscoverQuery message)
        {
            return new DiscoveryResponse(new List<EventSource> { new EventSource(EventType.DipswitchCode, EventDirections.Recieving),
                                                                 new EventSource(EventType.DipswitchCode, EventDirections.Sending)}, new PowerState());
        }
    }
}