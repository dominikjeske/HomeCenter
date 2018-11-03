using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Events;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Model.Messages.Queries.Service;
using HomeCenter.Model.ValueTypes;
using Proto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.InfraredBridge
{
    [ProxyCodeGenerator]
    public abstract class InfraredBridgeAdapter : Adapter
    {
        private const int DEAFULT_REPEAT = 3;
        private IntValue _pinNumber;
        private IntValue _I2cAddress;

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            _I2cAddress = this[AdapterProperties.I2cAddress].AsInt();

            //TODO register pin number?
            _pinNumber = this[AdapterProperties.PinNumber].AsInt();

            var registration = new SerialRegistrationCommand(Self, 3, new Format[]
               {
                new Format(1, typeof(byte), "System"),
                new Format(2, typeof(uint), "Code"),
                new Format(3, typeof(byte), "Bits")
               }
            );
            await MessageBroker.SendToService(registration).ConfigureAwait(false);
        }

        protected DiscoveryResponse Discover(DiscoverQuery message)
        {
            return new DiscoveryResponse(new List<EventSource> { new EventSource(EventType.InfraredCode, EventDirections.Recieving),
                                                                 new EventSource(EventType.InfraredCode, EventDirections.Sending)}, new PowerState());
        }

        protected Task Handle(SerialResultEvent serialResultCommand)
        {
            var system = serialResultCommand["System"].AsByte();
            var code = serialResultCommand["Code"].AsInt();

            return MessageBroker.PublisEvent(new InfraredEvent(Uid, system, code));
        }

        protected Task Handle(SendCodeCommand message)
        {
            var commandCode = message[CommandProperties.Code].AsUInt();
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

            var cmd = I2cCommand.Create(_I2cAddress, package.ToArray());
            return MessageBroker.SendToService(cmd);
        }
    }
}