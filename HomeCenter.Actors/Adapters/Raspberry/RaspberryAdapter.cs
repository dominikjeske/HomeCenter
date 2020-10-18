using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Contracts;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Model.Messages.Queries.Service;
using Proto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.PC
{
    [ProxyCodeGenerator]
    public abstract class RaspberryAdapter : Adapter
    {
        private readonly IGpioDevice _gpioDevice;

        protected RaspberryAdapter(IGpioDevice gpioDevice)
        {
            _gpioDevice = gpioDevice;
        }

        protected override Task OnStarted(IContext context)
        {
            ProtectResource(_gpioDevice.PinChanged.Subscribe(OnPinChanged));

            foreach (var pin in this.AsList(MessageProperties.PinChangeWithPullUp, new List<string>()))
            {
                _gpioDevice.RegisterPinChanged(int.Parse(pin), PinModes.InputPullUp);
            }

            //TODO Enumerable.Empty<string>() not working in .NET 3.0
            foreach (var pin in this.AsList(MessageProperties.PinChangeWithPullDown, new List<string>()))
            {
                _gpioDevice.RegisterPinChanged(int.Parse(pin), PinModes.InputPullDown);
            }

            return base.OnStarted(context);
        }

        protected DiscoveryResponse Discover(DiscoverQuery message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerState(),
                                                               new VolumeState()

                                          );
        }

        protected void Handle(TurnOnCommand command)
        {
            var pin = command.AsInt(MessageProperties.PinNumber);
            var reverse = command.AsBool(MessageProperties.ReversePinLevel);

            _gpioDevice.Write(pin, !reverse);
        }

        protected void Handle(TurnOffCommand command)
        {
            var pin = command.AsInt(MessageProperties.PinNumber);
            var reverse = command.AsBool(MessageProperties.ReversePinLevel);

            _gpioDevice.Write(pin, reverse);
        }

        protected void Handle(RegisterPinChangedCommand command)
        {
            var pinNumber = command.AsInt(MessageProperties.PinNumber);
            var pinMode = command.AsString(MessageProperties.PinMode);

            _gpioDevice.RegisterPinChanged(pinNumber, pinMode);
        }

        private Task OnPinChanged(PinChanged value)
        {
            return MessageBroker.Publish(PinValueChangedEvent.Create(Uid, value.PinNumber, value.IsRising));
        }

        protected void Handle(VolumeUpCommand command)
        {
            //TODO
        }

        protected void Handle(VolumeDownCommand command)
        {
            //TODO
        }

        protected void Handle(VolumeSetCommand command)
        {
            //TODO
        }
    }
}