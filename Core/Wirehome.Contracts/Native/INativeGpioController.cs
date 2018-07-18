namespace Wirehome.Core.Interface.Native
{
    public interface INativeGpioController
    {
        INativeGpio OpenPin(int pinNumber, NativeGpioSharingMode sharingMode);
    }
}