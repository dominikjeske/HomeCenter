using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Codes;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Events;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Model.Messages.Queries.Service;
using HomeCenter.Utils.Extensions;
using Microsoft.Extensions.Logging;
using Proto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.RemoteSocketBridge
{
    //TODO reset state of all devices on start?

    [ProxyCodeGenerator]
    public abstract class RemoteSocketBridgeAdapter : Adapter
    {
        private const int DEFAULT_REPEAT = 3;
        private int _pinNumber;
        private int _I2cAddress;

        private readonly Dictionary<string, bool> _state = new Dictionary<string, bool>();

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            _I2cAddress = AsInt(AdapterProperties.I2cAddress);
            _pinNumber = AsInt(AdapterProperties.PinNumber);

            var registration = new SerialRegistrationCommand(Self, 2, new Format[]
             {
                    new Format(1, typeof(uint), "Code"),
                    new Format(2, typeof(byte), "Bits"),
                    new Format(3, typeof(byte), "Protocol")
             });
            await MessageBroker.SendToService(registration).ConfigureAwait(false);
        }

        protected async Task Handle(SerialResultEvent serialResultCommand)
        {
            var code = serialResultCommand.AsUint("Code");
            var dipswitchCode = DipswitchCode.ParseCode(code);

            if (dipswitchCode == null)
            {
                Logger.LogWarning($"Unrecognized command parsed from code {code}");
                return;
            }

            await MessageBroker.PublisEvent(new DipswitchEvent(Uid, dipswitchCode)).ConfigureAwait(false);
        }

        protected async Task TurnOn(TurnOnCommand message)
        {
            byte[] package = PreparePackage(message, nameof(RemoteSocketCommand.TurnOn), out var dipswitchCode);
            var cmd = I2cCommand.Create(_I2cAddress, package);
            await MessageBroker.SendToService(cmd).ConfigureAwait(false);
            await UpdateState(dipswitchCode).ConfigureAwait(false);
        }

        protected async Task TurnOff(TurnOffCommand message)
        {
            byte[] package = PreparePackage(message, nameof(RemoteSocketCommand.TurnOff), out var dipswitchCode);
            var cmd = I2cCommand.Create(_I2cAddress, package);
            await MessageBroker.SendToService(cmd).ConfigureAwait(false);
            await UpdateState(dipswitchCode).ConfigureAwait(false);
        }

        private byte[] PreparePackage(Command message, string commandName, out DipswitchCode dipswitchCode)
        {
            var system = message.AsString(CommandProperties.System);
            var unit = message.AsString(CommandProperties.Unit);
            var repeat = message.AsInt(CommandProperties.Repeat, DEFAULT_REPEAT);
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
            bool oldValue = false; //TODO
            if(_state.ContainsKey(codeShortValue))
            {
                oldValue = _state[codeShortValue];
            }
            var newValue = code.Command == RemoteSocketCommand.TurnOn;

            _state[code.ToShortCode()] = await UpdateState(PowerState.StateName, oldValue, newValue).ConfigureAwait(false);
        }

        protected DiscoveryResponse Discover(DiscoverQuery message)
        {
            return new DiscoveryResponse(new List<EventSource> { new EventSource(EventType.DipswitchCode, EventDirections.Recieving),
                                                                 new EventSource(EventType.DipswitchCode, EventDirections.Sending)}, new PowerState());
        }
    }
}