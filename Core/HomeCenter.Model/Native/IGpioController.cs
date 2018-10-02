namespace HomeCenter.Model.Native
{
    public interface IGpioController
    {
        IGpio OpenPin(int pinNumber, GpioSharingMode sharingMode);
    }
}