namespace HomeCenter.Model.Contracts
{
    public interface IGpioDevice
    {
        void Write(int pin, bool value);
    }
}