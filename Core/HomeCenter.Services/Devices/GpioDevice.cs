using HomeCenter.Model.Contracts;
using System;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;

namespace HomeCenter.Services.Devices
{
    public class GpioDevice : IGpioDevice, IDisposable
    {
        private readonly GpioController _controller = new GpioController(PinNumberingScheme.Logical, new RaspberryPi3Driver());

        public void Dispose()
        {
            _controller.Dispose();
        }

        public void Write(int pin, bool value)
        {
            if (!_controller.IsPinOpen(pin))
            {
                _controller.OpenPin(pin, PinMode.Output);
            }
            _controller.Write(pin, value ? PinValue.High : PinValue.Low);
        }
    }
}