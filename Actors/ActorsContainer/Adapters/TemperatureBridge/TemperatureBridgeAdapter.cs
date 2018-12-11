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
        private readonly Dictionary<int, double> _state = new Dictionary<int, double>();

        protected TemperatureBridgeAdapter()
        {
            _requierdProperties.Add(MessageProperties.PinNumber);
        }

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            var _i2cAddress = AsInt(MessageProperties.I2cAddress);

            foreach (var val in AsList(MessageProperties.UsedPins))
            {
                _state.Add(int.Parse(val), 0);
            }
            var registration = new SerialRegistrationCommand(Self, 1, new Format[]
            {
                new Format(1, typeof(byte), "Pin"),
                new Format(2, typeof(float), "Temperature")
            });
            await MessageBroker.SendToService(registration).ConfigureAwait(false);
        }

        protected async Task Handle(SerialResultEvent serialResult)
        {
            var pin = serialResult.AsByte("Pin");
            var temperature = serialResult.AsDouble("Temperature");

            _state[pin] = await UpdateState(TemperatureState.StateName, pin, temperature).ConfigureAwait(false);
        }

        protected DiscoveryResponse Discover(DiscoverQuery message)
        {
            return new DiscoveryResponse(RequierdProperties(), new TemperatureState(ReadWriteMode.Read));
        }
    }
}