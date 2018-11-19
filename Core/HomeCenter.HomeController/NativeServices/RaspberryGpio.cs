using HomeCenter.Model.Native;
using System;
using Windows.Devices.Gpio;

namespace HomeCenter.HomeController.NativeServices
{
    internal class RaspberryGpio : IGpio
    {
        private readonly GpioPin _gpioPin;

        public event Action ValueChanged;

        public int PinNumber => _gpioPin.PinNumber;

        public RaspberryGpio(GpioPin gpioPin)
        {
            _gpioPin = gpioPin ?? throw new ArgumentNullException(nameof(gpioPin));
            gpioPin.ValueChanged += GpioPin_ValueChanged;
        }

        private void GpioPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args) => ValueChanged?.Invoke();

        public void SetDriveMode(Model.Native.GpioPinDriveMode pinMode) => _gpioPin.SetDriveMode((Windows.Devices.Gpio.GpioPinDriveMode)pinMode);

        public void Dispose() => _gpioPin.Dispose();

        public bool Read() => _gpioPin.Read() == GpioPinValue.High;

        public void Write(bool pinValue)
        {
            if (pinValue)
            {
                _gpioPin.Write(GpioPinValue.High);
            }
            else
            {
                _gpioPin.Write(GpioPinValue.Low);
            }
        }
    }
}