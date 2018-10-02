using Windows.Devices.Gpio;

namespace HomeCenter.Controller.NativeServices
{
    internal class RaspberryGpioController : IGpioController
    {
        private readonly GpioController _gpioController;

        public RaspberryGpioController() => _gpioController = GpioController.GetDefault();

        public IGpio OpenPin(int pinNumber, Core.Interface.Native.GpioSharingMode sharingMode) => new RaspberryGpio(_gpioController.OpenPin(pinNumber, (Windows.Devices.Gpio.GpioSharingMode)sharingMode));
    }
}