using System;
using Windows.Devices.Gpio;
using Wirehome.Core.Interface.Native;

namespace Wirehome.Raspberry
{
    internal class RaspberryGpio : INativeGpio
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

        public void SetDriveMode(NativeGpioPinDriveMode pinMode) => _gpioPin.SetDriveMode((GpioPinDriveMode)pinMode);

        public void Dispose() => _gpioPin.Dispose();

        public NativeGpioPinValue Read() => (NativeGpioPinValue)_gpioPin.Read();

        public void Write(NativeGpioPinValue pinValue) => _gpioPin.Write((GpioPinValue)pinValue);
    }
}
