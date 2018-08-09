using Windows.Devices.Gpio;
using HomeCenter.Core.Interface.Native;

namespace HomeCenter.Raspberry
{
    internal class RaspberryGpioController : IGpioController
    {
        private readonly GpioController _gpioController;

        public RaspberryGpioController() => _gpioController = GpioController.GetDefault();

        public IGpio OpenPin(int pinNumber, Core.Interface.Native.GpioSharingMode sharingMode) => new RaspberryGpio(_gpioController.OpenPin(pinNumber, (Windows.Devices.Gpio.GpioSharingMode)sharingMode));
    }
}
