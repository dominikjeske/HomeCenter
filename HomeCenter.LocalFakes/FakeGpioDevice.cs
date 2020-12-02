using HomeCenter.Abstractions;
using System;
using System.Reactive.Linq;

namespace HomeCenter.Fakes
{
    public class FakeGpioDevice : IGpioDevice
    {
        public IObservable<PinChanged> PinChanged => Observable.Empty<PinChanged>();

        public void Dispose()
        {
        }

        public void RegisterPinChanged(int pinNumber, string pinMode)
        {
        }

        public void Write(int pin, bool value)
        {
        }
    }
}