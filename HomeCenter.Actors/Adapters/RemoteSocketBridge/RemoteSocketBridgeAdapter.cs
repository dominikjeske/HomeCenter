using HomeCenter.Abstractions;
using HomeCenter.Actors.Core;
using HomeCenter.Adapters.RemoteSocketBridge.Codes;
using HomeCenter.Capabilities;
using HomeCenter.Messages.Commands.Device;
using HomeCenter.Messages.Commands.Service;
using HomeCenter.Messages.Events.Device;
using HomeCenter.Messages.Queries.Device;
using HomeCenter.Messages.Queries.Service;
using Microsoft.Extensions.Logging;
using Proto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.RemoteSocketBridge
{
    [Proxy]
    public class RemoteSocketBridgeAdapter : Adapter
    {
        private const int DEFAULT_REPEAT = 3;
        private int _pinNumber;
        private int _I2cAddress;

        private readonly Dictionary<string, bool> _state = new Dictionary<string, bool>();

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context);

            _I2cAddress = this.AsInt(MessageProperties.Address);
            _pinNumber = this.AsInt(MessageProperties.PinNumber);

            var registration = new RegisterSerialCommand(Self, 2, new Format[]
             {
                    new Format(1, typeof(uint), "Code"),
                    new Format(2, typeof(byte), "Bits"),
                    new Format(3, typeof(byte), "Protocol")
             });
            await MessageBroker.SendToService(registration);
        }

        protected async Task Handle(SerialResultEvent serialResultCommand)
        {
            var code = serialResultCommand.AsUint("Code");
            var dipswitchCode = DipswitchCode.ParseCode(code);

            if (dipswitchCode == null)
            {
                Logger.LogWarning("Unrecognized command parsed from code {code}", code);
                return;
            }

            await MessageBroker.Publish(DipswitchEvent.Create(Uid, dipswitchCode.Unit.ToString(), dipswitchCode.System.ToString(), dipswitchCode.Command.ToString()), Uid);
        }

        protected async Task TurnOn(TurnOnCommand message)
        {
            byte[] package = PreparePackage(message, nameof(RemoteSocketCommand.TurnOn), out var dipswitchCode);

            Logger.LogInformation("Sending code {code}", dipswitchCode.Code);

            var cmd = I2cCommand.Create(_I2cAddress, package);
            await MessageBroker.SendToService(cmd);
            await UpdateState(dipswitchCode);
        }

        protected async Task TurnOff(TurnOffCommand message)
        {
            byte[] package = PreparePackage(message, nameof(RemoteSocketCommand.TurnOff), out var dipswitchCode);
            var cmd = I2cCommand.Create(_I2cAddress, package);
            await MessageBroker.SendToService(cmd);
            await UpdateState(dipswitchCode);
        }

        private byte[] PreparePackage(Command message, string commandName, out DipswitchCode dipswitchCode)
        {
            var system = message.AsString(MessageProperties.System);
            var unit = message.AsString(MessageProperties.Unit);
            var repeat = message.AsInt(MessageProperties.Repeat, DEFAULT_REPEAT);
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
            var codeShortValue = code.ToShortCode();
            bool oldValue = false;
            if (_state.ContainsKey(codeShortValue))
            {
                oldValue = _state[codeShortValue];
            }
            var newValue = code.Command == RemoteSocketCommand.TurnOn;

            _state[code.ToShortCode()] = await UpdateState(PowerState.StateName, oldValue, newValue);
        }

        protected DiscoveryResponse Discover(DiscoverQuery message)
        {
            return new DiscoveryResponse(new PowerState());
        }
    }
}