using HomeCenter.Model.Native;
using Windows.Devices.Gpio;

namespace HomeCenter.HomeController.NativeServices
{
    internal class RaspberryGpioController : IGpioController
    {
        private readonly GpioController _gpioController;

        public RaspberryGpioController() => _gpioController = GpioController.GetDefault();

        public IGpio OpenPin(int pinNumber, Model.Native.GpioSharingMode sharingMode) => new RaspberryGpio(_gpioController.OpenPin(pinNumber, (Windows.Devices.Gpio.GpioSharingMode)sharingMode));
    }
}