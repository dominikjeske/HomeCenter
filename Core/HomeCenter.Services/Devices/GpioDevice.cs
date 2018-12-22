using HomeCenter.Model.Contracts;
using System;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Threading;
using System.Threading.Tasks;

namespace HomeCenter.Services.Devices
{
    public class GpioDevice : IGpioDevice, IDisposable
    {
        private readonly GpioController _controller = new GpioController(PinNumberingScheme.Logical, new RaspberryPi3Driver());

        private PinValue _value;

        public GpioDevice()
        {
            _controller.OpenPin(21, PinMode.Input);
           

            _controller.RegisterCallbackForPinValueChangedEvent(21, PinEventTypes.Falling, Falling);
            _controller.RegisterCallbackForPinValueChangedEvent(21, PinEventTypes.Rising, Rising);

            Task.Run(() =>
            {
                while(true)
                {
                    var value = _controller.Read(21);

                   // if(value != _value)
                   // {
                        Console.WriteLine($"VALUE CAHANGED: FROM {_value} to {value}");
                        _value = value;
                  //  }

                    Thread.Sleep(1000);
                }
            });
        }

        public void Falling(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!! FALLING");
        }

        public void Rising(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!! RISING");
        }

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