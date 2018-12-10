using System.Device.Gpio;

namespace HomeCenter.Services.Devices
{
    public class GpioDevice
    {
        public void Write(int pin)
        {
            using (var controller = new GpioController())
            {
                controller.OpenPin(pin, PinMode.Output);
                controller.Write(pin, PinValue.High);
            }
        }
    }
}