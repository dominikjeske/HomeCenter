using HomeCenter.Abstractions;
using HomeCenter.Abstractions.Defaults;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace HomeCenter.Devices
{
    public class GpioDevice : IGpioDevice
    {
        private readonly GpioController _controller = new GpioController(PinNumberingScheme.Logical, new RaspberryPi3Driver());
        private readonly List<int> _changeHandlers = new List<int>();
        private readonly Subject<PinChanged> _pinChanged = new Subject<PinChanged>();

        public IObservable<PinChanged> PinChanged => _pinChanged.AsObservable();

        public void RegisterPinChanged(int pinNumber, string pinMode)
        {
            if (_changeHandlers.Contains(pinNumber)) return;

            _controller.OpenPin(pinNumber, GetPinMode(pinMode));

            _controller.RegisterCallbackForPinValueChangedEvent(pinNumber, PinEventTypes.Falling, PinFalling);
            _controller.RegisterCallbackForPinValueChangedEvent(pinNumber, PinEventTypes.Rising, PinRising);
        }

        public void Write(int pin, bool value)
        {
            if (!_controller.IsPinOpen(pin))
            {
                _controller.OpenPin(pin, PinMode.Output);
            }
            _controller.Write(pin, value ? PinValue.High : PinValue.Low);
        }

        public void Dispose()
        {
            foreach (var pin in _changeHandlers)
            {
                _controller.UnregisterCallbackForPinValueChangedEvent(pin, PinFalling);
                _controller.UnregisterCallbackForPinValueChangedEvent(pin, PinRising);
            }

            _controller.Dispose();
            _pinChanged.Dispose();
        }

        private void PinFalling(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            _pinChanged.OnNext(GetPinChanged(pinValueChangedEventArgs));
        }

        private void PinRising(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            _pinChanged.OnNext(GetPinChanged(pinValueChangedEventArgs));
        }

        private PinChanged GetPinChanged(PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            return new PinChanged
            {
                PinNumber = pinValueChangedEventArgs.PinNumber,
                IsRising = pinValueChangedEventArgs.ChangeType == PinEventTypes.Rising
            };
        }

        private PinMode GetPinMode(string pinMode)
        {
            switch (pinMode)
            {
                case PinModes.Input:
                    return PinMode.Input;

                case PinModes.Output:
                    return PinMode.Output;

                case PinModes.InputPullDown:
                    return PinMode.InputPullDown;

                case PinModes.InputPullUp:
                    return PinMode.InputPullUp;
            }

            throw new ArgumentException($"Pin mode {pinMode} is unsupported");
        }
    }
}