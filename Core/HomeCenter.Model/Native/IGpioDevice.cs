namespace HomeCenter.Model.Devices
{
    public interface IGpioDevice
    {
        void Write(int pin, bool value);
    }
}