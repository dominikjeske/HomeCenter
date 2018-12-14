using HomeCenter.Model.Contracts;
using HomeCenter.Services.Bootstrapper;
using HomeCenter.Services.Controllers;
using HomeCenter.Services.Devices;
using SimpleInjector;

namespace HomeCenter.Runner
{
    public class RemoteWirehomeBootstrapper : Bootstrapper
    {
        private readonly string _configuration;

        public RemoteWirehomeBootstrapper(Container container, string configuration) : base(container)
        {
            _configuration = configuration;
            _container.Options.AllowOverridingRegistrations = true;
        }

        public Container Container => _container;

        protected override void RegisterNativeServices()
        {
            _container.RegisterSingleton<II2cBus, I2cBus>();
            _container.RegisterSingleton<ISerialDevice, SerialDevice>();
            _container.RegisterSingleton<IGpioDevice, GpioDevice>();
        }

        protected override void RegisterControllerOptions()
        {
            _container.RegisterInstance(new ControllerOptions
            {
                AdapterMode = "Embedded",
                RemoteActorPort = 8000,
                Configuration = _configuration
            });
        }
    }
}