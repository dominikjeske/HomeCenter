using Windows.Devices.Gpio;
using HomeCenter.Core.Interface.Native;

namespace HomeCenter.Raspberry
{
    internal class RaspberryGpioController : INativeGpioController
    {
        private readonly GpioController _gpioController;

        public RaspberryGpioController() => _gpioController = GpioController.GetDefault();

        public INativeGpio OpenPin(int pinNumber, NativeGpioSharingMode sharingMode) => new RaspberryGpio(_gpioController.OpenPin(pinNumber, (GpioSharingMode)sharingMode));
    }
}
