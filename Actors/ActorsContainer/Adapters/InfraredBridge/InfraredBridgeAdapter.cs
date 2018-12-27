using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Model.Messages.Queries.Service;
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
        private int _I2cAddress;

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            _I2cAddress = AsInt(MessageProperties.Address);

            var registration = new RegisterSerialCommand(Self, 3, new Format[]
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
            return new DiscoveryResponse(new InfraredReceiverState(), new InfraredSenderState());
        }

        protected Task Handle(SerialResultEvent serialResultCommand)
        {
            var system = serialResultCommand.AsByte("System");
            var code = serialResultCommand.AsUint("Code");

            return MessageBroker.PublishEvent(InfraredEvent.Create(Uid, system, code));
        }

        protected Task Handle(SendCodeCommand message)
        {
            var commandCode = message.AsUint(MessageProperties.Code);
            var system = message.AsInt(MessageProperties.System);
            var bits = message.AsInt(MessageProperties.Bits);
            var repeat = message.AsInt(MessageProperties.Repeat, DEAFULT_REPEAT);

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