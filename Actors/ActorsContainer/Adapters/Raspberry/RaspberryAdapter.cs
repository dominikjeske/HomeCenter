using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Devices;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Queries.Device;

namespace HomeCenter.Adapters.PC
{
    [ProxyCodeGenerator]
    public abstract class RaspberryAdapter : Adapter
    {
        private readonly IGpioDevice _gpioDevice;

        protected RaspberryAdapter(IGpioDevice gpioDevice)
        {
            _gpioDevice = gpioDevice;
        }

        protected DiscoveryResponse Discover(DiscoverQuery message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerState(),
                                                               new VolumeState()

                                          );
        }

        protected void Handle(TurnOnCommand command)
        {
            var pin = command.AsInt(MessageProperties.PinNumber);
            var reverse = command.AsBool(MessageProperties.ReversePinLevel);

            _gpioDevice.Write(pin, !reverse);
        }

        protected void Handle(TurnOffCommand command)
        {
            var pin = command.AsInt(MessageProperties.PinNumber);
            var reverse = command.AsBool(MessageProperties.ReversePinLevel);

            _gpioDevice.Write(pin, reverse);
        }

        protected void Handle(VolumeUpCommand command)
        {
            //TODO
        }

        protected void Handle(VolumeDownCommand command)
        {
            //TODO
        }

        protected void Handle(VolumeSetCommand command)
        {
            //TODO
        }
    }
}