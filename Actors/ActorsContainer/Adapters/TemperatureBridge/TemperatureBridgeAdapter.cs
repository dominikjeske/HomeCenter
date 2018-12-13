using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Model.Messages.Queries.Service;
using Proto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.TemperatureBridge
{
    [ProxyCodeGenerator]
    public abstract class TemperatureBridgeAdapter : Adapter
    {
        private const int I2C_ACTION_TEMPERATURE = 1;
        private readonly Dictionary<int, double> _state = new Dictionary<int, double>();
        private int _i2cAddress;

        protected TemperatureBridgeAdapter()
        {
            _requierdProperties.Add(MessageProperties.PinNumber);
        }

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            _i2cAddress = AsInt(MessageProperties.I2cAddress);

            foreach (var val in AsList(MessageProperties.UsedPins))
            {
                _state.Add(int.Parse(val), 0);
            }
            var registration = new SerialRegistrationCommand(Self, 1, new Format[]
            {
                new Format(1, typeof(byte), MessageProperties.PinNumber),
                new Format(2, typeof(float), MessageProperties.Value)
            });
            await MessageBroker.SendToService(registration).ConfigureAwait(false);
        }

        protected async Task Handle(SerialResultEvent serialResult)
        {
            var pin = serialResult.AsByte(MessageProperties.PinNumber);
            var temperature = serialResult.AsDouble(MessageProperties.Value);

            _state[pin] = await UpdateState(TemperatureState.StateName, pin, temperature).ConfigureAwait(false);
        }

        protected DiscoveryResponse Discover(DiscoverQuery message)
        {
            RegisterPinNumber(message);

            return new DiscoveryResponse(RequierdProperties(), new TemperatureState(ReadWriteMode.Read));
        }

        private void RegisterPinNumber(DiscoverQuery message)
        {
            var pin = message.AsByte(MessageProperties.PinNumber);
            var registrationMessage = new byte[] { I2C_ACTION_TEMPERATURE, pin };
            MessageBroker.SendToService(I2cCommand.Create(_i2cAddress, registrationMessage));
        }
    }
}