namespace HomeCenter.Core.Interface.Native
{
    public interface IGpioController
    {
        IGpio OpenPin(int pinNumber, GpioSharingMode sharingMode);
    }
}