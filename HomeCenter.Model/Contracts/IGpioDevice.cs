using System;

namespace HomeCenter.Model.Contracts
{
    public interface IGpioDevice : IDisposable
    {
        IObservable<PinChanged> PinChanged { get; }

        void Write(int pin, bool value);

        void RegisterPinChanged(int pinNumber, string pinMode);
    }
}