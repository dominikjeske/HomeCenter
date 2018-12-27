using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Contracts;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Model.Messages.Queries.Service;
using Microsoft.Extensions.Logging;
using Proto;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.PC
{
    [ProxyCodeGenerator]
    public abstract class RaspberryAdapter : Adapter, IObserver<PinChanged>
    {
        private readonly IGpioDevice _gpioDevice;

        protected RaspberryAdapter(IGpioDevice gpioDevice)
        {
            _gpioDevice = gpioDevice;

            _disposables.Add(_gpioDevice.PinChanged.Subscribe(this));
        }

        protected override Task OnStarted(IContext context)
        {
            foreach (var pin in AsList(MessageProperties.PinChangeWithPullUp, Enumerable.Empty<string>()))
            {
                _gpioDevice.RegisterPinChanged(int.Parse(pin), PinModes.InputPullUp);
            }

            foreach (var pin in AsList(MessageProperties.PinChangeWithPullDown, Enumerable.Empty<string>()))
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

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
            Logger.LogError($"{error}");
        }

        public void OnNext(PinChanged value)
        {
            MessageBroker.Send(PinValueChangedEvent.Create(Uid, value.PinNumber, value.IsRising), "HSPE16InputOnly_1");

            //MessageBroker.PublishEvent(PinValueChangedEvent.Create(Uid, value.PinNumber, value.IsRising)).GetAwaiter().GetResult();
        }
    }
}