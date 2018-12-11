using HomeCenter.CodeGeneration;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Devices;
using HomeCenter.Model.Messages.Commands.Device;

namespace HomeCenter.Services.Networking
{
    [ProxyCodeGenerator]
    public abstract class GpioService : Service
    {
        private readonly IGpioDevice _gpioDevice;

        public GpioService(IGpioDevice gpioDevice)
        {
            _gpioDevice = gpioDevice;
        }

        protected void Handle(TurnOnCommand command)
        {
            var pin = command.AsInt(AdapterProperties.PinNumber);
            _gpioDevice.Write(pin, true);
        }

        protected void Handle(TurnOffCommand command)
        {
            var pin = command.AsInt(AdapterProperties.PinNumber);
            _gpioDevice.Write(pin, true);
        }
    }
}