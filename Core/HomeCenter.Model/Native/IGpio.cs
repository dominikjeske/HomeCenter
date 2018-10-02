using System;

namespace HomeCenter.Model.Native
{
    public interface IGpio : IDisposable
    {
        event Action ValueChanged;

        void SetDriveMode(GpioPinDriveMode pinMode);

        bool Read();

        void Write(bool pinValue);

        int PinNumber { get; }
    }
}